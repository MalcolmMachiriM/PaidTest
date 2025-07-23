using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ProcessPaymentRequest
    {
        [Required]
        public int PaymentAccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        public string? ExternalTransactionId { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [EmailAddress]
        public string? CustomerEmail { get; set; }

        [StringLength(100)]
        public string? CustomerName { get; set; }

        public Dictionary<string, object>? Metadata { get; set; }
    }
}
