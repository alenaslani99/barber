using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record OwnerResult(Guid Id, string FirstName, string LastName, string Email, string Phone);

public sealed class GetOwnerHandler(TenantContext db)
{
    public async Task<OwnerResult?> HandleAsync(CancellationToken ct = default) =>
        await db.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Owner)
            .OrderBy(u => u.CreatedAt)
            .Select(u => new OwnerResult(u.Id, u.FirstName, u.LastName, u.Email, u.Phone))
            .FirstOrDefaultAsync(ct);
}
