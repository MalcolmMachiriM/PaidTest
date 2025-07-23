using System.ComponentModel.DataAnnotations;
using System.Transactions;

namespace API.Models
{
    public class PaymentAccount
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        [Required]
        [StringLength(100)]
        public string AccountName { get; set; }

        public PaymentProvider Provider { get; set; }

        [Required]
        public string ApiKey { get; set; }

        public string? SecretKey { get; set; }

        public string? WebhookUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsTestMode { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Tenant Tenant { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
    public enum PaymentProvider
    {
        Stripe = 1,
        PayPal = 2,
        Square = 3,
        Razorpay = 4
    }
}
