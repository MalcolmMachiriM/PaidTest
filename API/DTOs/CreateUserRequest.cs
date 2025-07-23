using API.Models;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CreateUserRequest
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        public UserRole? Role { get; set; }
    }
}
