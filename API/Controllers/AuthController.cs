using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITenantService _tenantService;
        private readonly IConfiguration _configuration;

        public AuthController(IUserService userService, ITenantService tenantService, IConfiguration configuration)
        {
            _userService = userService;
            _tenantService = tenantService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<object>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _userService.AuthenticateAsync(request.Email, request.Password);
                if (user == null)
                {
                    return BadRequest(ApiResponse.Error("Invalid credentials"));
                }

                var token = GenerateJwtToken(user);

                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    Token = token,
                    User = new UserResponse
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Role = user.Role,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt
                    }
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error("Login failed", new List<string> { ex.Message }));
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> Register([FromBody] CreateUserRequest request)
        {
            try
            {
                var currentTenantId = _tenantService.GetCurrentTenantId();
                if (currentTenantId == 0)
                {
                    return BadRequest(ApiResponse<UserResponse>.ErrorResult("Invalid tenant"));
                }

                var user = await _userService.CreateUserAsync(request);
                var response = new UserResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };

                return Ok(ApiResponse<UserResponse>.SuccessResult(response, "User created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<UserResponse>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserResponse>.ErrorResult("Registration failed", new List<string> { ex.Message }));
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetCurrentUser()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(ApiResponse<UserResponse>.ErrorResult("User not found"));
                }

                var response = new UserResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                };

                return Ok(ApiResponse<UserResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserResponse>.ErrorResult("Failed to get user", new List<string> { ex.Message }));
            }
        }

        private string GenerateJwtToken(Models.User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "your-secret-key");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("TenantId", user.TenantId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
