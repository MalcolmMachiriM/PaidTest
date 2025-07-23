using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class TenantSettings
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TransactionFeePercentage { get; set; } = 2.9m;

        [Column(TypeName = "decimal(10,2)")]
        public decimal FixedTransactionFee { get; set; } = 0.30m;

        [StringLength(3)]
        public string DefaultCurrency { get; set; } = "USD";

        public bool AllowRefunds { get; set; } = true;

        public int MaxRefundDays { get; set; } = 30;

        [StringLength(500)]
        public string? CustomDomainUrl { get; set; }

        [StringLength(1000)]
        public string? WebhookEndpoints { get; set; } // JSON array

        public bool EnableEmailNotifications { get; set; } = true;

        public string? ThemeSettings { get; set; } // JSON string for UI customization

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Tenant Tenant { get; set; }
    }
}
