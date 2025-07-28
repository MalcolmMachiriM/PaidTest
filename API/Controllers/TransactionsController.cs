using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public TransactionsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("process")]
        public async Task<ActionResult<ApiResponse<TransactionResponse>>> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            try
            {
                var transaction = await _paymentService.ProcessPaymentAsync(request);
                var response = MapToTransactionResponse(transaction);

                return Ok(ApiResponse<TransactionResponse>.SuccessResult(response, "Payment processed successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<TransactionResponse>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TransactionResponse>.ErrorResult("Failed to process payment", new List<string> { ex.Message }));
            }
        }

        [HttpPost("{id}/refund")]
        public async Task<ActionResult<ApiResponse<TransactionResponse>>> RefundTransaction(int id, [FromBody] RefundRequest request)
        {
            try
            {
                var transaction = await _paymentService.RefundTransactionAsync(id, request.Amount);
                var response = MapToTransactionResponse(transaction);

                return Ok(ApiResponse<TransactionResponse>.SuccessResult(response, "Refund processed successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<TransactionResponse>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<TransactionResponse>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TransactionResponse>.ErrorResult("Failed to process refund", new List<string> { ex.Message }));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionResponse>>>> GetTransactions(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var transactions = await _paymentService.GetTransactionsAsync(from, to);
                var response = transactions.Select(MapToTransactionResponse);

                return Ok(ApiResponse<IEnumerable<TransactionResponse>>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<TransactionResponse>>.ErrorResult("Failed to get transactions", new List<string> { ex.Message }));
            }
        }

        [HttpGet("{transactionId}")]
        public async Task<ActionResult<ApiResponse<TransactionResponse>>> GetTransaction(string transactionId)
        {
            try
            {
                var transaction = await _paymentService.GetTransactionAsync(transactionId);
                if (transaction == null)
                {
                    return NotFound(ApiResponse<TransactionResponse>.ErrorResult("Transaction not found"));
                }

                var response = MapToTransactionResponse(transaction);
                return Ok(ApiResponse<TransactionResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TransactionResponse>.ErrorResult("Failed to get transaction", new List<string> { ex.Message }));
            }
        }

        private TransactionResponse MapToTransactionResponse(Models.Transaction transaction)
        {
            return new TransactionResponse
            {
                Id = transaction.Id,
                TransactionId = transaction.TransactionId,
                ExternalTransactionId = transaction.ExternalTransactionId,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                Status = transaction.Status,
                Type = transaction.Type,
                Description = transaction.Description,
                CustomerEmail = transaction.CustomerEmail,
                CustomerName = transaction.CustomerName,
                CreatedAt = transaction.CreatedAt,
                ProcessedAt = transaction.ProcessedAt,
                PaymentAccount = new PaymentAccountResponse
                {
                    Id = transaction.PaymentAccount.Id,
                    AccountName = transaction.PaymentAccount.AccountName,
                    Provider = transaction.PaymentAccount.Provider,
                    ApiKey = transaction.PaymentAccount.ApiKey,
                    WebhookUrl = transaction.PaymentAccount.WebhookUrl,
                    IsActive = transaction.PaymentAccount.IsActive,
                    IsTestMode = transaction.PaymentAccount.IsTestMode,
                    CreatedAt = transaction.PaymentAccount.CreatedAt
                }
            };
        }
    }
}
