using Barber.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record BarberListItem(Guid Id, string FirstName, string LastName, string Seniority, int SeniorityLevel);

public sealed class GetBarbersHandler(TenantContext db)
{
    public async Task<List<BarberListItem>> HandleAsync(CancellationToken ct = default)
    {
        List<BarberListItem> items = await (
            from staff in db.Staff.AsNoTracking()
            join user in db.Users.AsNoTracking() on staff.UserId equals user.Id
            join seniority in db.Seniorities.AsNoTracking() on staff.SeniorityId equals seniority.Id
            where staff.IsActive
            orderby seniority.Level descending, user.FirstName
            select new BarberListItem(staff.Id, user.FirstName, user.LastName, seniority.Name, seniority.Level)
        ).ToListAsync(ct);
        return items;
    }
}
