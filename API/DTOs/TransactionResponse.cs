using API.Models;

namespace API.DTOs
{
    public class TransactionResponse
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public string ExternalTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public TransactionStatus Status { get; set; }
        public TransactionType Type { get; set; }
        public string? Description { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public PaymentAccountResponse PaymentAccount { get; set; }
    }
}
