using Barber.Application.Auth;
using Barber.Application.Exceptions;
using Barber.Application.Logging;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record CreateOwnerCommand(string FirstName, string LastName, string Email, string Phone);

/// <summary>
/// The only place a plain-text password leaves the API. It is shown once to the admin and
/// never stored or logged.
/// </summary>
public sealed record CredentialsResult(Guid UserId, string FirstName, string LastName, string Email, string Password);

public sealed class CreateOwnerHandler(TenantContext db, IPasswordService passwords, AuditWriter audit)
{
    public async Task<CredentialsResult> HandleAsync(CreateOwnerCommand command, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(u => u.Role == UserRole.Owner, ct))
            throw new ConflictException("This shop already has an owner.");

        if (await db.Users.AnyAsync(u => u.Email == command.Email, ct))
            throw new DuplicateEmailException();

        if (await db.Users.AnyAsync(u => u.Phone == command.Phone, ct))
            throw new DuplicatePhoneException();

        string password = PasswordGenerator.Generate();
        User owner = new()
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            Phone = command.Phone,
            PasswordHash = string.Empty,
            Role = UserRole.Owner
        };
        owner.PasswordHash = passwords.HashPassword(owner, password);
        db.Users.Add(owner);
        await db.SaveChangesAsync(ct);

        await audit.WriteAsync("admin.owner.create", AuditWriter.AdminActor, null, true, null, "User", owner.Id, ct);
        return new CredentialsResult(owner.Id, owner.FirstName, owner.LastName, owner.Email, password);
    }
}
