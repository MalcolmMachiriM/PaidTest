using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;

namespace API.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        public int PaymentAccountId { get; set; }

        [Required]
        [StringLength(100)]
        public string TransactionId { get; set; }

        [Required]
        [StringLength(100)]
        public string ExternalTransactionId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; }

        public TransactionStatus Status { get; set; }

        public TransactionType Type { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? CustomerEmail { get; set; }

        [StringLength(100)]
        public string? CustomerName { get; set; }

        public string? Metadata { get; set; } // JSON string

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        // Navigation properties
        public virtual Tenant Tenant { get; set; }
        public virtual PaymentAccount PaymentAccount { get; set; }
    }
    public enum TransactionStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4,
        Refunded = 5
    }

    public enum TransactionType
    {
        Payment = 1,
        Refund = 2,
        Chargeback = 3
    }
}
