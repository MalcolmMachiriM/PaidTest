using API.DTOs;
using API.Models;

namespace API.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(CreateUserRequest request);
        Task<User?> AuthenticateAsync(string email, string password);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<User> UpdateUserAsync(int id, UpdateUserRequest request);
        Task DeleteUserAsync(int id);
        Task<IEnumerable<User>> GetTenantUsersAsync();
    }
}
