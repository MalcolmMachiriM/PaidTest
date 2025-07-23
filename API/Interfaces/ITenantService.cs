using API.Models;

namespace API.Interfaces
{
    public interface ITenantService
    {
        int GetCurrentTenantId();
        string GetCurrentTenantSubdomain();
        Task<bool> TenantExistsAsync(string subdomain);
        Task<Tenant?> GetTenantBySubdomainAsync(string subdomain);
        Task<Tenant> CreateTenantAsync(CreateTenantRequest request);
    }
}
