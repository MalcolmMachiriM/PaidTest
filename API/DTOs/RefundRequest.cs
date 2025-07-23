using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RefundRequest
    {
        [Range(0.01, double.MaxValue)]
        public decimal? Amount { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}
