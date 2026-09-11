using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Barber.Api.Core;

public sealed class TenantProvider : ITenantProvider
{
    public const string HeaderName = "X-Tenant-Slug";
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(1);

    private readonly IHttpContextAccessor _http;
    private readonly CatalogContext _catalog;
    private readonly IMemoryCache _cache;
    private readonly string _template;

    public TenantProvider(
        IHttpContextAccessor http,
        CatalogContext catalog,
        IMemoryCache cache,
        IConfiguration configuration)
    {
        _http = http;
        _catalog = catalog;
        _cache = cache;
        _template = configuration.GetConnectionString("TenantTemplate")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:TenantTemplate.");
    }

    public string GetConnectionString() => $"{_template};Database={GetTenant().DatabaseName}";

    public string GetSlug()
    {
        string? slug = _http.HttpContext?.Request.Headers[HeaderName].ToString();
        if (string.IsNullOrWhiteSpace(slug))
            throw new TenantNotFoundException(slug ?? string.Empty);
        return slug;
    }

    private Tenant GetTenant()
    {
        string? slug = _http.HttpContext?.Request.Headers[HeaderName].ToString();
        if (string.IsNullOrWhiteSpace(slug))
            throw new TenantNotFoundException(slug ?? string.Empty);

        Tenant tenant = _cache.GetOrCreate($"tenant:{slug}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheLifetime;
            return Load(slug);
        })!;
        return tenant;
    }

    private Tenant Load(string slug) =>
        _catalog.Tenants.AsNoTracking().FirstOrDefault(t => t.Slug == slug && t.IsActive)
            ?? throw new TenantNotFoundException(slug);
}
