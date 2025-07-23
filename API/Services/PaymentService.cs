using API.Data;
using API.DTOs;
using API.Interfaces;
using API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace API.Services
{
    public class PaymentService: IPaymentService
    {
        private readonly PaymentGatewayDbContext _context;
        private readonly ITenantService _tenantService;

        public PaymentService(PaymentGatewayDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        public async Task<PaymentAccount> CreatePaymentAccountAsync(CreatePaymentAccountRequest request)
        {
            var account = new PaymentAccount
            {
                TenantId = _tenantService.GetCurrentTenantId(),
                AccountName = request.AccountName,
                Provider = request.Provider,
                ApiKey = request.ApiKey,
                SecretKey = request.SecretKey,
                WebhookUrl = request.WebhookUrl,
                IsTestMode = request.IsTestMode
            };

            _context.PaymentAccounts.Add(account);
            await _context.SaveChangesAsync();

            return account;
        }

        public async Task<Transaction> ProcessPaymentAsync(ProcessPaymentRequest request)
        {
            var account = await GetPaymentAccountAsync(request.PaymentAccountId);
            if (account == null)
                throw new ArgumentException("Payment account not found");

            var transaction = new Transaction
            {
                TenantId = _tenantService.GetCurrentTenantId(),
                PaymentAccountId = request.PaymentAccountId,
                TransactionId = Guid.NewGuid().ToString(),
                ExternalTransactionId = request.ExternalTransactionId ?? Guid.NewGuid().ToString(),
                Amount = request.Amount,
                Currency = request.Currency,
                Status = TransactionStatus.Pending,
                Type = TransactionType.Payment,
                Description = request.Description,
                CustomerEmail = request.CustomerEmail,
                CustomerName = request.CustomerName,
                Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Here you would integrate with actual payment providers
            // For now, we'll simulate a successful payment
            transaction.Status = TransactionStatus.Completed;
            transaction.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<Transaction> RefundTransactionAsync(int transactionId, decimal? amount = null)
        {
            var originalTransaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.Type == TransactionType.Payment);

            if (originalTransaction == null)
                throw new ArgumentException("Original transaction not found");

            if (originalTransaction.Status != TransactionStatus.Completed)
                throw new InvalidOperationException("Can only refund completed transactions");

            var refundAmount = amount ?? originalTransaction.Amount;
            if (refundAmount > originalTransaction.Amount)
                throw new ArgumentException("Refund amount cannot exceed original amount");

            var refundTransaction = new Transaction
            {
                TenantId = originalTransaction.TenantId,
                PaymentAccountId = originalTransaction.PaymentAccountId,
                TransactionId = Guid.NewGuid().ToString(),
                ExternalTransactionId = Guid.NewGuid().ToString(),
                Amount = refundAmount,
                Currency = originalTransaction.Currency,
                Status = TransactionStatus.Completed,
                Type = TransactionType.Refund,
                Description = $"Refund for transaction {originalTransaction.TransactionId}",
                CustomerEmail = originalTransaction.CustomerEmail,
                CustomerName = originalTransaction.CustomerName,
                ProcessedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(refundTransaction);
            await _context.SaveChangesAsync();

            return refundTransaction;
        }

        public async Task<Transaction?> GetTransactionAsync(string transactionId)
        {
            return await _context.Transactions
                .Include(t => t.PaymentAccount)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = _context.Transactions.Include(t => t.PaymentAccount).AsQueryable();

            if (from.HasValue)
                query = query.Where(t => t.CreatedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(t => t.CreatedAt <= to.Value);

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<PaymentAccount?> GetPaymentAccountAsync(int id)
        {
            return await _context.PaymentAccounts
                .FirstOrDefaultAsync(pa => pa.Id == id && pa.IsActive);
        }

        public async Task<IEnumerable<PaymentAccount>> GetPaymentAccountsAsync()
        {
            return await _context.PaymentAccounts
                .Where(pa => pa.IsActive)
                .OrderBy(pa => pa.AccountName)
                .ToListAsync();
        }
    }
}
