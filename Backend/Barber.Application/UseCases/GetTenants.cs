using Barber.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed class GetTenantsHandler(CatalogContext catalog)
{
    public async Task<List<TenantResult>> HandleAsync(CancellationToken ct = default) =>
        await catalog.Tenants
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TenantResult(t.Id, t.Slug, t.DatabaseName, t.IsActive, t.CreatedAt))
            .ToListAsync(ct);
}
