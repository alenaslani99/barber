using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record TenantSetupResult(
    TenantResult Tenant,
    bool HasShop,
    bool HasOwner,
    int StaffCount,
    int ServiceCount);

/// <summary>
/// Reports how far onboarding got for one tenant so the admin wizard can resume.
/// </summary>
public sealed class GetTenantSetupHandler(CatalogContext catalog, TenantDatabases databases)
{
    public async Task<TenantSetupResult?> HandleAsync(string slug, CancellationToken ct = default)
    {
        Tenant? tenant = await catalog.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == slug, ct);
        if (tenant is null)
            return null;

        await using TenantContext db = databases.Open(tenant.DatabaseName);
        bool hasShop = await db.Barbershops.AnyAsync(ct);
        bool hasOwner = await db.Users.AnyAsync(u => u.Role == UserRole.Owner, ct);
        int staffCount = await db.Staff.CountAsync(ct);
        int serviceCount = await db.Services.CountAsync(ct);

        TenantResult result = new(tenant.Id, tenant.Slug, tenant.DatabaseName, tenant.IsActive, tenant.CreatedAt);
        return new TenantSetupResult(result, hasShop, hasOwner, staffCount, serviceCount);
    }
}
