using Barber.Application.Logging;
using Barber.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record UpdateStaffCommand(Guid Id, bool IsActive, Guid OwnerId, string OwnerEmail);

public sealed class UpdateStaffHandler(TenantContext db, AuditWriter audit)
{
    public async Task<Domain.Staff?> HandleAsync(UpdateStaffCommand command, CancellationToken ct = default)
    {
        Domain.Staff? staff = await db.Staff.FirstOrDefaultAsync(s => s.Id == command.Id, ct);
        if (staff is null)
            return null;

        staff.IsActive = command.IsActive;
        await db.SaveChangesAsync(ct);
        await audit.WriteAsync("staff.update", command.OwnerEmail, command.OwnerId, true, null, "Staff", staff.Id, ct);
        return staff;
    }
}
