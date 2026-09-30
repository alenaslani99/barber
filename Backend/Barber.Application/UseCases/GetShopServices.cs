using Barber.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

/// <summary>
/// Every service including inactive ones, for the admin app. The public
/// <see cref="GetServicesHandler"/> only returns active services.
/// </summary>
public sealed class GetShopServicesHandler(TenantContext db)
{
    public async Task<List<ServiceDetailResult>> HandleAsync(CancellationToken ct = default) =>
        await db.Services
            .AsNoTracking()
            .OrderBy(s => s.Price)
            .ThenBy(s => s.Name)
            .Select(s => new ServiceDetailResult(s.Id, s.Name, s.Price, s.DurationMinutes, s.SlotMinutes, s.IsActive))
            .ToListAsync(ct);
}
