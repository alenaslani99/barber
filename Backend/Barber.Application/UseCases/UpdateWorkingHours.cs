using Barber.Application.Logging;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record WorkingHoursInput(DayOfWeek Day, TimeOnly Open, TimeOnly Close, bool IsClosed);
public sealed record UpdateWorkingHoursCommand(Guid BarbershopId, List<WorkingHoursInput> Hours, Guid OwnerId, string OwnerEmail);

public sealed class UpdateWorkingHoursHandler(TenantContext db, AuditWriter audit)
{
    public async Task<bool> HandleAsync(UpdateWorkingHoursCommand command, CancellationToken ct = default)
    {
        bool exists = await db.Barbershops.AsNoTracking().AnyAsync(s => s.Id == command.BarbershopId, ct);
        if (!exists)
            return false;

        List<WorkingHours> current = await db.WorkingHours
            .Where(h => h.BarbershopId == command.BarbershopId)
            .ToListAsync(ct);
        db.WorkingHours.RemoveRange(current);

        foreach (WorkingHoursInput input in command.Hours)
        {
            db.WorkingHours.Add(new WorkingHours
            {
                BarbershopId = command.BarbershopId,
                Day = input.Day,
                Open = input.Open,
                Close = input.Close,
                IsClosed = input.IsClosed
            });
        }

        await db.SaveChangesAsync(ct);
        await audit.WriteAsync("hours.update", command.OwnerEmail, command.OwnerId, true, null, "Barbershop", command.BarbershopId, ct);
        return true;
    }
}
