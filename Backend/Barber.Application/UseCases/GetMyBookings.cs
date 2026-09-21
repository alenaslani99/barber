using Barber.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record GetMyBookingsQuery(Guid UserId, int Skip, int Take);
public sealed record MyBookingItem(Guid Id, string ServiceName, string BarberName, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string Status);
public sealed record MyBookingsResult(List<MyBookingItem> Items, int Total);

public sealed class GetMyBookingsHandler(TenantContext db)
{
    public async Task<MyBookingsResult> HandleAsync(GetMyBookingsQuery query, CancellationToken ct = default)
    {
        IQueryable<MyBookingItem> baseQuery =
            from booking in db.Bookings.AsNoTracking()
            join service in db.Services.AsNoTracking() on booking.ServiceId equals service.Id
            join staff in db.Staff.AsNoTracking() on booking.StaffId equals staff.Id
            join barber in db.Users.AsNoTracking() on staff.UserId equals barber.Id
            where booking.UserId == query.UserId
            orderby booking.StartsAt descending
            select new MyBookingItem(
                booking.Id,
                service.Name,
                barber.FirstName + " " + barber.LastName,
                booking.StartsAt,
                booking.EndsAt,
                booking.Status.ToString());

        int total = await baseQuery.CountAsync(ct);
        List<MyBookingItem> items = await baseQuery.Skip(query.Skip).Take(query.Take).ToListAsync(ct);
        return new MyBookingsResult(items, total);
    }
}
