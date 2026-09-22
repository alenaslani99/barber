using Barber.Application.Exceptions;
using Barber.Application.Logging;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record UpdateBookingStatusCommand(Guid BookingId, Guid UserId, bool OwnerView, string Status);

public sealed class UpdateBookingStatusHandler(TenantContext db, AuditWriter audit)
{
    private static readonly Dictionary<BookingStatus, BookingStatus[]> Transitions = new()
    {
        [BookingStatus.Pending] = [BookingStatus.Confirmed, BookingStatus.Cancelled],
        [BookingStatus.Confirmed] = [BookingStatus.Completed, BookingStatus.Cancelled, BookingStatus.NoShow],
    };

    public async Task<BookingListItem?> HandleAsync(UpdateBookingStatusCommand command, CancellationToken ct = default)
    {
        Booking? booking = await db.Bookings.FirstOrDefaultAsync(b => b.Id == command.BookingId, ct);
        if (booking is null)
            return null;

        if (!command.OwnerView)
        {
            bool owns = await db.Staff
                .AsNoTracking()
                .AnyAsync(s => s.Id == booking.StaffId && s.UserId == command.UserId && s.IsActive, ct);
            if (!owns)
                return null;
        }

        if (!Enum.TryParse<BookingStatus>(command.Status, true, out BookingStatus next))
            throw new BookingConflictException($"Unknown status '{command.Status}'.");
        if (!Transitions.TryGetValue(booking.Status, out BookingStatus[]? allowed) || !allowed.Contains(next))
            throw new BookingConflictException($"Cannot move booking from {booking.Status} to {next}.");

        booking.Status = next;
        await db.SaveChangesAsync(ct);

        BookingListItem? item = await (
            from b in db.Bookings.AsNoTracking()
            join service in db.Services.AsNoTracking() on b.ServiceId equals service.Id
            join staff in db.Staff.AsNoTracking() on b.StaffId equals staff.Id
            join barber in db.Users.AsNoTracking() on staff.UserId equals barber.Id
            join client in db.Users.AsNoTracking() on b.UserId equals client.Id
            where b.Id == booking.Id
            select new BookingListItem(
                b.Id,
                service.Name,
                barber.FirstName + " " + barber.LastName,
                client.FirstName + " " + client.LastName,
                b.StartsAt,
                b.EndsAt,
                b.Status.ToString())
        ).FirstOrDefaultAsync(ct);

        User? user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == command.UserId, ct);
        await audit.WriteAsync("booking.status_changed", user?.Email ?? string.Empty, command.UserId, true, null, "Booking", booking.Id, ct);
        return item;
    }
}
