using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Barber.Api.Core;

public sealed class TenantProvider : ITenantProvider
{
    public const string HeaderName = "X-Tenant-Slug";
    public const string RouteKey = "tenantSlug";
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
        string? slug = ResolveSlug();
        if (string.IsNullOrWhiteSpace(slug))
            throw new TenantNotFoundException(slug ?? string.Empty);
        return slug;
    }

    /// <summary>
    /// Client apps send the slug as a header. Admin routes carry it in the path as
    /// <c>{tenantSlug}</c> instead, so the regular tenant handlers work unchanged there.
    /// </summary>
    private string? ResolveSlug()
    {
        HttpContext? context = _http.HttpContext;
        if (context is null)
            return null;

        string header = context.Request.Headers[HeaderName].ToString();
        if (!string.IsNullOrWhiteSpace(header))
            return header;

        return context.GetRouteValue(RouteKey) as string;
    }

    private Tenant GetTenant()
    {
        string slug = GetSlug();

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
