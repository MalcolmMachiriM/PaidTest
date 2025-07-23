using API.Models;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CreatePaymentAccountRequest
    {
        [Required]
        [StringLength(100)]
        public string AccountName { get; set; }

        [Required]
        public PaymentProvider Provider { get; set; }

        [Required]
        public string ApiKey { get; set; }

        public string? SecretKey { get; set; }

        public string? WebhookUrl { get; set; }

        public bool IsTestMode { get; set; } = true;
    }
}
