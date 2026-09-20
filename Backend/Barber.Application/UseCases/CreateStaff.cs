using Barber.DataAccess;
using Barber.DataAccess.Configurations;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record CreateStaffCommand(string Email, Guid BarbershopId, Guid? SeniorityId);

public sealed class CreateStaffHandler(TenantContext db)
{
    public async Task<Staff> HandleAsync(CreateStaffCommand command, CancellationToken ct = default)
    {
        User? user = await db.Users.FirstOrDefaultAsync(u => u.Email == command.Email, ct);
        if (user is null)
            throw new KeyNotFoundException($"User '{command.Email}' not found. They must register first.");

        bool shopExists = await db.Barbershops.AnyAsync(b => b.Id == command.BarbershopId, ct);
        if (!shopExists)
            throw new KeyNotFoundException($"Barbershop '{command.BarbershopId}' not found.");

        Staff? existing = await db.Staff
            .FirstOrDefaultAsync(s => s.UserId == user.Id && s.BarbershopId == command.BarbershopId, ct);
        if (existing is not null)
        {
            if (user.Role != UserRole.Barber)
            {
                user.Role = UserRole.Barber;
                await db.SaveChangesAsync(ct);
            }
            return existing;
        }

        Guid seniorityId = command.SeniorityId ?? SeniorityConfiguration.BarberId;
        bool seniorityExists = await db.Seniorities.AnyAsync(s => s.Id == seniorityId, ct);
        if (!seniorityExists)
            throw new KeyNotFoundException($"Seniority '{seniorityId}' not found.");

        Staff staff = new()
        {
            BarbershopId = command.BarbershopId,
            UserId = user.Id,
            SeniorityId = seniorityId
        };
        user.Role = UserRole.Barber;
        db.Staff.Add(staff);
        await db.SaveChangesAsync(ct);
        return staff;
    }
}
