using API.Models;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class UpdateUserRequest
    {
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public UserRole? Role { get; set; }
    }
}
