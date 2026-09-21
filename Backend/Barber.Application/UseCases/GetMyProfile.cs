using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record GetMyProfileQuery(Guid UserId);
public sealed record UserProfileResult(Guid Id, string FirstName, string LastName, string Email, string Phone);

public sealed class GetMyProfileHandler(TenantContext db)
{
    public async Task<UserProfileResult?> HandleAsync(GetMyProfileQuery query, CancellationToken ct = default)
    {
        User? user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == query.UserId, ct);
        if (user is null)
            return null;
        return new UserProfileResult(user.Id, user.FirstName, user.LastName, user.Email, user.Phone);
    }
}
