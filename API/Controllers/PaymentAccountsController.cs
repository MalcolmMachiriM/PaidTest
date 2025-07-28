using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentAccountsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentAccountsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PaymentAccountResponse>>> CreatePaymentAccount([FromBody] CreatePaymentAccountRequest request)
        {
            try
            {
                var account = await _paymentService.CreatePaymentAccountAsync(request);

                var response = new PaymentAccountResponse
                {
                    Id = account.Id,
                    AccountName = account.AccountName,
                    Provider = account.Provider,
                    ApiKey = account.ApiKey,
                    WebhookUrl = account.WebhookUrl,
                    IsActive = account.IsActive,
                    IsTestMode = account.IsTestMode,
                    CreatedAt = account.CreatedAt
                };

                return Ok(ApiResponse<PaymentAccountResponse>.SuccessResult(response, "Payment account created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaymentAccountResponse>.ErrorResult("Failed to create payment account", new List<string> { ex.Message }));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentAccountResponse>>>> GetPaymentAccounts()
        {
            try
            {
                var accounts = await _paymentService.GetPaymentAccountsAsync();
                var response = accounts.Select(a => new PaymentAccountResponse
                {
                    Id = a.Id,
                    AccountName = a.AccountName,
                    Provider = a.Provider,
                    ApiKey = a.ApiKey,
                    WebhookUrl = a.WebhookUrl,
                    IsActive = a.IsActive,
                    IsTestMode = a.IsTestMode,
                    CreatedAt = a.CreatedAt
                });

                return Ok(ApiResponse<IEnumerable<PaymentAccountResponse>>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<PaymentAccountResponse>>.ErrorResult("Failed to get payment accounts", new List<string> { ex.Message }));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PaymentAccountResponse>>> GetPaymentAccount(int id)
        {
            try
            {
                var account = await _paymentService.GetPaymentAccountAsync(id);
                if (account == null)
                {
                    return NotFound(ApiResponse<PaymentAccountResponse>.ErrorResult("Payment account not found"));
                }

                var response = new PaymentAccountResponse
                {
                    Id = account.Id,
                    AccountName = account.AccountName,
                    Provider = account.Provider,
                    ApiKey = account.ApiKey,
                    WebhookUrl = account.WebhookUrl,
                    IsActive = account.IsActive,
                    IsTestMode = account.IsTestMode,
                    CreatedAt = account.CreatedAt
                };

                return Ok(ApiResponse<PaymentAccountResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaymentAccountResponse>.ErrorResult("Failed to get payment account", new List<string> { ex.Message }));
            }
        }
    }
}
