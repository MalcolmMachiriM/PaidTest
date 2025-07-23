using API.Models;

namespace API.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentAccount> CreatePaymentAccountAsync(CreatePaymentAccountRequest request);
        Task<Transaction> ProcessPaymentAsync(ProcessPaymentRequest request);
        Task<Transaction> RefundTransactionAsync(int transactionId, decimal? amount = null);
        Task<Transaction?> GetTransactionAsync(string transactionId);
        Task<IEnumerable<Transaction>> GetTransactionsAsync(DateTime? from = null, DateTime? to = null);
        Task<PaymentAccount?> GetPaymentAccountAsync(int id);
        Task<IEnumerable<PaymentAccount>> GetPaymentAccountsAsync();
    }
}
