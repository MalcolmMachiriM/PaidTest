using API.Data;
using API.Interfaces;
using API.Models;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace API.Services

{
    public class UserService
    {
        private readonly PaymentGatewayDbContext _context;
        private readonly ITenantService _tenantService;

        public UserService(PaymentGatewayDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        public async Task<User> CreateUserAsync(CreateUserRequest request)
        {
            if (await EmailExistsAsync(request.Email))
                throw new InvalidOperationException("Email already exists");

            var user = new User
            {
                TenantId = _tenantService.GetCurrentTenantId(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email.ToLower(),
                PasswordHash = BC.HashPassword(request.Password),
                Role = request.Role ?? UserRole.User
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email.ToLower() && u.IsActive);

            if (user == null || !BC.Verify(password, user.PasswordHash))
                return null;

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Email == email.ToLower() && u.IsActive);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email.ToLower());
        }

        public async Task<User> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
                throw new ArgumentException("User not found");

            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;

            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                if (await EmailExistsAsync(request.Email))
                    throw new InvalidOperationException("Email already exists");

                user.Email = request.Email.ToLower();
            }

            if (request.Role.HasValue)
                user.Role = request.Role.Value;

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
                throw new ArgumentException("User not found");

            user.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetTenantUsersAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }
    }
}
