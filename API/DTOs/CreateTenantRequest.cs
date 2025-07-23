using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CreateTenantRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Subdomain can only contain lowercase letters, numbers, and hyphens")]
        public string Subdomain { get; set; }

        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; }

        [Phone]
        public string? ContactPhone { get; set; }
    }
}
