using Barber.Application.Logging;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record UpsertShopCommand(
    string Name,
    string? Address,
    string? Phone,
    string Tagline,
    string Description,
    string TimeZone);

/// <summary>
/// Creates the tenant's barbershop on first call (with default opening hours) and
/// updates its details on later calls. One shop per tenant.
/// </summary>
public sealed class UpsertShopHandler(TenantContext db, AuditWriter audit)
{
    private static readonly TimeOnly DefaultOpen = new(9, 0);
    private static readonly TimeOnly DefaultClose = new(19, 0);

    public async Task<ShopDetailsResult> HandleAsync(UpsertShopCommand command, CancellationToken ct = default)
    {
        Barbershop? shop = await db.Barbershops.OrderBy(s => s.Name).FirstOrDefaultAsync(ct);
        bool created = shop is null;
        if (shop is null)
        {
            shop = new Barbershop { Name = command.Name };
            db.Barbershops.Add(shop);
            AddDefaultHours(shop.Id);
        }

        shop.Name = command.Name;
        shop.Address = string.IsNullOrWhiteSpace(command.Address) ? null : command.Address.Trim();
        shop.Phone = string.IsNullOrWhiteSpace(command.Phone) ? null : command.Phone.Trim();
        shop.Tagline = command.Tagline;
        shop.Description = command.Description;
        shop.TimeZone = command.TimeZone;
        await db.SaveChangesAsync(ct);

        string action = created ? "admin.shop.create" : "admin.shop.update";
        await audit.WriteAsync(action, AuditWriter.AdminActor, null, true, null, "Barbershop", shop.Id, ct);
        return ShopDetailsResult.From(shop);
    }

    // Mon-Sat 09:00-19:00, Sunday closed. The owner adjusts these from their own app.
    private void AddDefaultHours(Guid barbershopId)
    {
        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            bool closed = day == DayOfWeek.Sunday;
            db.WorkingHours.Add(new WorkingHours
            {
                BarbershopId = barbershopId,
                Day = day,
                Open = closed ? TimeOnly.MinValue : DefaultOpen,
                Close = closed ? TimeOnly.MinValue : DefaultClose,
                IsClosed = closed
            });
        }
    }
}
