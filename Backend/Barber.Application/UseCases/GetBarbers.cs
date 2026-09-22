using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record BarberListItem(Guid Id, string FirstName, string LastName, string Seniority, int SeniorityLevel, bool IsActive);

public sealed class GetBarbersHandler(TenantContext db)
{
    public async Task<List<BarberListItem>> HandleAsync(bool includeInactive, CancellationToken ct = default)
    {
        IQueryable<Staff> staff = db.Staff.AsNoTracking();
        if (!includeInactive)
            staff = staff.Where(s => s.IsActive);

        List<BarberListItem> items = await (
            from s in staff
            join user in db.Users.AsNoTracking() on s.UserId equals user.Id
            join seniority in db.Seniorities.AsNoTracking() on s.SeniorityId equals seniority.Id
            orderby seniority.Level descending, user.FirstName
            select new BarberListItem(s.Id, user.FirstName, user.LastName, seniority.Name, seniority.Level, s.IsActive)
        ).ToListAsync(ct);
        return items;
    }
}
