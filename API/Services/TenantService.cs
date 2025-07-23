using API.Data;
using API.DTOs;
using API.Interfaces;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class TenantService : ITenantService
    {
        private readonly PaymentGatewayDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private int? _currentTenantId;

        public TenantService(PaymentGatewayDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetCurrentTenantId()
        {
            if (_currentTenantId.HasValue)
                return _currentTenantId.Value;

            var subdomain = GetCurrentTenantSubdomain();
            if (string.IsNullOrEmpty(subdomain))
                return 0;

            var tenant = _context.Tenants
                .AsNoTracking()
                .FirstOrDefault(t => t.Subdomain == subdomain && t.IsActive);

            _currentTenantId = tenant?.Id ?? 0;
            return _currentTenantId.Value;
        }

        public string GetCurrentTenantSubdomain()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return string.Empty;

            var host = httpContext.Request.Host.Host;
            var parts = host.Split('.');

            // Extract subdomain (assuming format: subdomain.domain.com)
            if (parts.Length >= 3)
                return parts[0];

            return string.Empty;
        }

        public async Task<bool> TenantExistsAsync(string subdomain)
        {
            return await _context.Tenants
                .AsNoTracking()
                .AnyAsync(t => t.Subdomain == subdomain);
        }

        public async Task<Tenant?> GetTenantBySubdomainAsync(string subdomain)
        {
            return await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Subdomain == subdomain && t.IsActive);
        }

        public async Task<Tenant> CreateTenantAsync(CreateTenantRequest request)
        {
            if (await TenantExistsAsync(request.Subdomain))
                throw new InvalidOperationException("Subdomain already exists");

            var tenant = new Tenant
            {
                Name = request.Name,
                Subdomain = request.Subdomain.ToLower(),
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                IsActive = true
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // Create default settings for the tenant
            var settings = new TenantSettings
            {
                TenantId = tenant.Id
            };

            _context.TenantSettings.Add(settings);
            await _context.SaveChangesAsync();

            return tenant;
        }
    }
}
