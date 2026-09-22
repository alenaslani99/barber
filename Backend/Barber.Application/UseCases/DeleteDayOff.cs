using Barber.Application.Logging;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record DeleteDayOffCommand(Guid Id, Guid OwnerId, string OwnerEmail);

public sealed class DeleteDayOffHandler(TenantContext db, AuditWriter audit)
{
    public async Task<bool> HandleAsync(DeleteDayOffCommand command, CancellationToken ct = default)
    {
        WorkingDayOverride? dayOff = await db.WorkingDayOverrides
            .FirstOrDefaultAsync(o => o.Id == command.Id, ct);
        if (dayOff is null)
            return false;

        db.WorkingDayOverrides.Remove(dayOff);
        await db.SaveChangesAsync(ct);
        await audit.WriteAsync("dayoff.delete", command.OwnerEmail, command.OwnerId, true, null, "WorkingDayOverride", dayOff.Id, ct);
        return true;
    }
}
