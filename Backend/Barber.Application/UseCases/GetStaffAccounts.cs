using Barber.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record StaffAccountItem(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Seniority,
    bool IsActive);

/// <summary>
/// Staff with their login details for the admin app. The public barber list
/// (<see cref="GetBarbersHandler"/>) deliberately leaves contact data out.
/// </summary>
public sealed class GetStaffAccountsHandler(TenantContext db)
{
    public async Task<List<StaffAccountItem>> HandleAsync(CancellationToken ct = default) =>
        await (
            from s in db.Staff.AsNoTracking()
            join user in db.Users.AsNoTracking() on s.UserId equals user.Id
            join seniority in db.Seniorities.AsNoTracking() on s.SeniorityId equals seniority.Id
            orderby seniority.Level descending, user.FirstName
            select new StaffAccountItem(
                s.Id, user.Id, user.FirstName, user.LastName, user.Email, user.Phone, seniority.Name, s.IsActive)
        ).ToListAsync(ct);
}
