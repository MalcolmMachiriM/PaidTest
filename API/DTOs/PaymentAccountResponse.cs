using API.Models;

namespace API.DTOs
{
    public class PaymentAccountResponse
    {
        public int Id { get; set; }
        public string AccountName { get; set; }
        public PaymentProvider Provider { get; set; }
        public string ApiKey { get; set; }
        public string? WebhookUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsTestMode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
