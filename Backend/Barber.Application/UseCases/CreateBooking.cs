using Barber.Application.Exceptions;
using Barber.Application.Logging;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record CreateBookingCommand(Guid UserId, Guid ServiceId, Guid StaffId, DateTimeOffset StartsAt, string? Notes);
public sealed record BookingResult(Guid Id, string Status, DateTimeOffset StartsAt, DateTimeOffset EndsAt);

public sealed class CreateBookingHandler(TenantContext db, AuditWriter audit)
{
    public async Task<BookingResult> HandleAsync(CreateBookingCommand command, CancellationToken ct = default)
    {
        Service? service = await db.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == command.ServiceId && s.IsActive, ct);
        if (service is null)
            throw new KeyNotFoundException($"Service '{command.ServiceId}' not found.");

        Staff? staff = await db.Staff
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == command.StaffId && s.IsActive, ct);
        if (staff is null)
            throw new KeyNotFoundException($"Staff '{command.StaffId}' not found.");

        if (staff.BarbershopId != service.BarbershopId)
            throw new BookingConflictException("Service and staff belong to different shops.");

        DateTimeOffset endsAt = command.StartsAt.AddMinutes(service.DurationMinutes);

        bool staffBusy = await db.Bookings.AsNoTracking().AnyAsync(b =>
            b.StaffId == staff.Id &&
            b.Status != BookingStatus.Cancelled &&
            b.StartsAt < endsAt &&
            command.StartsAt < b.EndsAt, ct);
        if (staffBusy)
            throw new BookingConflictException("The selected slot is no longer free.");

        bool userBusy = await db.Bookings.AsNoTracking().AnyAsync(b =>
            b.UserId == command.UserId &&
            b.Status != BookingStatus.Cancelled &&
            b.StartsAt < endsAt &&
            command.StartsAt < b.EndsAt, ct);
        if (userBusy)
            throw new BookingConflictException("You already have a booking in this slot.");

        Booking booking = new()
        {
            BarbershopId = service.BarbershopId,
            ServiceId = service.Id,
            StaffId = staff.Id,
            UserId = command.UserId,
            StartsAt = command.StartsAt,
            EndsAt = endsAt,
            Notes = command.Notes
        };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);

        User? user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == command.UserId, ct);
        await audit.WriteAsync("booking.create", user?.Email ?? string.Empty, command.UserId, true, null, "Booking", booking.Id, ct);

        return new BookingResult(booking.Id, booking.Status.ToString(), booking.StartsAt, booking.EndsAt);
    }
}
