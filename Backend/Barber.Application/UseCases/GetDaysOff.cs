using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record GetDaysOffQuery(Guid? StaffId, DateOnly From);
public sealed record DayOffItem(Guid Id, Guid? StaffId, string StaffName, DateOnly Date, string? Reason);

public sealed class GetDaysOffHandler(TenantContext db)
{
    public async Task<List<DayOffItem>> HandleAsync(GetDaysOffQuery query, CancellationToken ct = default)
    {
        IQueryable<WorkingDayOverride> filtered = db.WorkingDayOverrides.AsNoTracking().Where(o => o.Date >= query.From);
        if (query.StaffId.HasValue)
            filtered = filtered.Where(o => o.StaffId == query.StaffId.Value);

        List<DayOffItem> items = await (
            from o in filtered
            join staff in db.Staff.AsNoTracking() on o.StaffId equals staff.Id into staffJoin
            from staff in staffJoin.DefaultIfEmpty()
            join user in db.Users.AsNoTracking() on staff.UserId equals user.Id into userJoin
            from user in userJoin.DefaultIfEmpty()
            orderby o.Date
            select new DayOffItem(
                o.Id,
                o.StaffId,
                user == null ? "SVI" : user.FirstName + " " + user.LastName,
                o.Date,
                o.Reason)
        ).ToListAsync(ct);
        return items;
    }
}
