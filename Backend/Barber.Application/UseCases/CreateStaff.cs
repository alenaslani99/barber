using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record CreateStaffCommand(string Email, Guid BarbershopId);

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

        Client? client = await db.Clients.FirstOrDefaultAsync(c => c.Email == command.Email, ct);
        Staff staff = new()
        {
            BarbershopId = command.BarbershopId,
            UserId = user.Id,
            FirstName = client?.FirstName ?? command.Email,
            LastName = client?.LastName ?? string.Empty
        };
        user.Role = UserRole.Barber;
        db.Staff.Add(staff);
        await db.SaveChangesAsync(ct);
        return staff;
    }
}
