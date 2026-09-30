using Barber.Application.Auth;
using Barber.Application.Exceptions;
using Barber.Application.Logging;
using Barber.DataAccess;
using Barber.DataAccess.Configurations;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record CreateBarberAccountCommand(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    Guid? SeniorityId);

/// <summary>
/// Admin-side barber onboarding: creates the login (generated password) and the staff
/// record in one save. Unlike <see cref="CreateStaffHandler"/>, the barber does not have
/// to register first.
/// </summary>
public sealed class CreateBarberAccountHandler(TenantContext db, IPasswordService passwords, AuditWriter audit)
{
    public async Task<CredentialsResult> HandleAsync(CreateBarberAccountCommand command, CancellationToken ct = default)
    {
        Guid? shopId = await db.Barbershops
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(ct);
        if (shopId is null)
            throw new ConflictException("Create the shop (step 2) before adding staff.");

        Guid seniorityId = command.SeniorityId ?? SeniorityConfiguration.BarberId;
        if (!await db.Seniorities.AnyAsync(s => s.Id == seniorityId, ct))
            throw new KeyNotFoundException($"Seniority '{seniorityId}' not found.");

        if (await db.Users.AnyAsync(u => u.Email == command.Email, ct))
            throw new DuplicateEmailException();

        if (await db.Users.AnyAsync(u => u.Phone == command.Phone, ct))
            throw new DuplicatePhoneException();

        string password = PasswordGenerator.Generate();
        User user = new()
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            Phone = command.Phone,
            PasswordHash = string.Empty,
            Role = UserRole.Barber
        };
        user.PasswordHash = passwords.HashPassword(user, password);
        Staff staff = new()
        {
            BarbershopId = shopId.Value,
            UserId = user.Id,
            SeniorityId = seniorityId
        };
        db.Users.Add(user);
        db.Staff.Add(staff);
        await db.SaveChangesAsync(ct);

        await audit.WriteAsync("admin.staff.create", AuditWriter.AdminActor, null, true, null, "Staff", staff.Id, ct);
        return new CredentialsResult(user.Id, user.FirstName, user.LastName, user.Email, password);
    }
}
