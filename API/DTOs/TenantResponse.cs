namespace API.DTOs
{
    public class TenantResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Subdomain { get; set; }
        public string ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
