using Barber.Application.Exceptions;
using Barber.Application.Logging;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record CreateDayOffCommand(Guid StaffId, DateOnly Date, string? Reason, Guid OwnerId, string OwnerEmail);

public sealed class CreateDayOffHandler(TenantContext db, AuditWriter audit)
{
    public async Task<Guid> HandleAsync(CreateDayOffCommand command, CancellationToken ct = default)
    {
        Staff? staff = await db.Staff
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == command.StaffId && s.IsActive, ct);
        if (staff is null)
            throw new KeyNotFoundException($"Staff '{command.StaffId}' not found.");

        bool exists = await db.WorkingDayOverrides.AsNoTracking().AnyAsync(o =>
            o.Date == command.Date && o.StaffId == staff.Id, ct);
        if (exists)
            throw new BookingConflictException("Day off already exists.");

        WorkingDayOverride dayOff = new()
        {
            BarbershopId = staff.BarbershopId,
            StaffId = staff.Id,
            Date = command.Date,
            Reason = command.Reason
        };
        db.WorkingDayOverrides.Add(dayOff);
        await db.SaveChangesAsync(ct);
        await audit.WriteAsync("dayoff.create", command.OwnerEmail, command.OwnerId, true, null, "WorkingDayOverride", dayOff.Id, ct);
        return dayOff.Id;
    }
}
