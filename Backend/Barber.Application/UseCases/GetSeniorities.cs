using Barber.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record SeniorityItem(Guid Id, string Name, int Level);

public sealed class GetSenioritiesHandler(TenantContext db)
{
    public async Task<List<SeniorityItem>> HandleAsync(CancellationToken ct = default) =>
        await db.Seniorities
            .AsNoTracking()
            .OrderBy(s => s.Level)
            .Select(s => new SeniorityItem(s.Id, s.Name, s.Level))
            .ToListAsync(ct);
}
