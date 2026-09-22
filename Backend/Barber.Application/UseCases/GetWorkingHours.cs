using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record ShopHoursRow(DayOfWeek Day, TimeOnly Open, TimeOnly Close, bool IsClosed);

public sealed class GetWorkingHoursHandler(TenantContext db)
{
    public async Task<List<ShopHoursRow>?> HandleAsync(Guid barbershopId, CancellationToken ct = default)
    {
        bool exists = await db.Barbershops.AsNoTracking().AnyAsync(s => s.Id == barbershopId, ct);
        if (!exists)
            return null;

        List<ShopHoursRow> rows = await db.WorkingHours
            .AsNoTracking()
            .Where(h => h.BarbershopId == barbershopId)
            .OrderBy(h => h.Day)
            .Select(h => new ShopHoursRow(h.Day, h.Open, h.Close, h.IsClosed))
            .ToListAsync(ct);
        return rows;
    }
}
