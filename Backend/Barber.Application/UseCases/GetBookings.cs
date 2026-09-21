using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record GetBookingsQuery(
    Guid UserId,
    bool OwnerView,
    List<Guid> StaffIds,
    List<BookingStatus> Statuses,
    bool Upcoming,
    int Skip,
    int Take);
public sealed record BookingListItem(
    Guid Id,
    string ServiceName,
    string BarberName,
    string ClientName,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Status);
public sealed record BookingsResult(List<BookingListItem> Items, int Total);

public sealed class GetBookingsHandler(TenantContext db)
{
    public async Task<BookingsResult> HandleAsync(GetBookingsQuery query, CancellationToken ct = default)
    {
        List<Guid> staffIds = query.StaffIds;
        if (!query.OwnerView)
        {
            staffIds = await db.Staff
                .AsNoTracking()
                .Where(s => s.UserId == query.UserId && s.IsActive)
                .Select(s => s.Id)
                .ToListAsync(ct);
            if (staffIds.Count == 0)
                return new BookingsResult([], 0);
        }

        IQueryable<Booking> filtered = db.Bookings.AsNoTracking();
        if (staffIds.Count > 0)
            filtered = filtered.Where(b => staffIds.Contains(b.StaffId));
        if (query.Statuses.Count > 0)
            filtered = filtered.Where(b => query.Statuses.Contains(b.Status));
        if (query.Upcoming)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            filtered = filtered.Where(b =>
                b.StartsAt >= now &&
                (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed));
        }

        IQueryable<BookingListItem> rows =
            from booking in filtered
            join service in db.Services.AsNoTracking() on booking.ServiceId equals service.Id
            join staff in db.Staff.AsNoTracking() on booking.StaffId equals staff.Id
            join barber in db.Users.AsNoTracking() on staff.UserId equals barber.Id
            join client in db.Users.AsNoTracking() on booking.UserId equals client.Id
            orderby booking.StartsAt descending
            select new BookingListItem(
                booking.Id,
                service.Name,
                barber.FirstName + " " + barber.LastName,
                client.FirstName + " " + client.LastName,
                booking.StartsAt,
                booking.EndsAt,
                booking.Status.ToString());

        int total = await rows.CountAsync(ct);
        List<BookingListItem> items = await rows.Skip(query.Skip).Take(query.Take).ToListAsync(ct);
        return new BookingsResult(items, total);
    }
}
