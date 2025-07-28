using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly IUserService _userService;

        public TenantsController(ITenantService tenantService, IUserService userService)
        {
            _tenantService = tenantService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> CreateTenant([FromBody] CreateTenantRequest request)
        {
            try
            {
                var tenant = await _tenantService.CreateTenantAsync(request);

                var response = new TenantResponse
                {
                    Id = tenant.Id,
                    Name = tenant.Name,
                    Subdomain = tenant.Subdomain,
                    ContactEmail = tenant.ContactEmail,
                    ContactPhone = tenant.ContactPhone,
                    IsActive = tenant.IsActive,
                    CreatedAt = tenant.CreatedAt
                };

                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    Tenant = response,
                    Message = "Tenant created successfully. You can now access your portal at: " + tenant.Subdomain + ".yourdomain.com"
                }));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to create tenant", new List<string> { ex.Message }));
            }
        }

        [HttpGet("check-subdomain/{subdomain}")]
        public async Task<ActionResult<ApiResponse<object>>> CheckSubdomain(string subdomain)
        {
            try
            {
                var exists = await _tenantService.TenantExistsAsync(subdomain);
                return Ok(ApiResponse<object>.SuccessResult(new { Available = !exists }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to check subdomain", new List<string> { ex.Message }));
            }
        }

        [HttpGet("current")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<TenantResponse>>> GetCurrentTenant()
        {
            try
            {
                var subdomain = _tenantService.GetCurrentTenantSubdomain();
                var tenant = await _tenantService.GetTenantBySubdomainAsync(subdomain);

                if (tenant == null)
                {
                    return NotFound(ApiResponse<TenantResponse>.ErrorResult("Tenant not found"));
                }

                var response = new TenantResponse
                {
                    Id = tenant.Id,
                    Name = tenant.Name,
                    Subdomain = tenant.Subdomain,
                    ContactEmail = tenant.ContactEmail,
                    ContactPhone = tenant.ContactPhone,
                    IsActive = tenant.IsActive,
                    CreatedAt = tenant.CreatedAt
                };

                return Ok(ApiResponse<TenantResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TenantResponse>.ErrorResult("Failed to get tenant", new List<string> { ex.Message }));
            }
        }
    }
}
