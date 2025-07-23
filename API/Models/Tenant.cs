using System.ComponentModel.DataAnnotations;
using System.Transactions;

namespace API.Models
{
    public class Tenant
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Subdomain { get; set; }

        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; }

        public string? ContactPhone { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<PaymentAccount> PaymentAccounts { get; set; } = new List<PaymentAccount>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual TenantSettings? Settings { get; set; }
    }
}
