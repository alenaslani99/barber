using Barber.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record ServiceListItem(Guid Id, string Name, int DurationMinutes, decimal Price);

public sealed class GetServicesHandler(TenantContext db)
{
    public async Task<List<ServiceListItem>> HandleAsync(CancellationToken ct = default)
    {
        List<ServiceListItem> items = await db.Services
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Price)
            .Select(s => new ServiceListItem(s.Id, s.Name, s.DurationMinutes, s.Price))
            .ToListAsync(ct);
        return items;
    }
}
