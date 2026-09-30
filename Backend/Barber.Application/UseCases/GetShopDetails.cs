using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record ShopDetailsResult(
    Guid Id,
    string Name,
    string? Address,
    string? Phone,
    string Tagline,
    string Description,
    string TimeZone)
{
    public static ShopDetailsResult From(Barbershop shop) =>
        new(shop.Id, shop.Name, shop.Address, shop.Phone, shop.Tagline, shop.Description, shop.TimeZone);
}

/// <summary>
/// Editable shop fields for the admin form. The public <see cref="GetShopHandler"/>
/// returns the booking-page shape instead.
/// </summary>
public sealed class GetShopDetailsHandler(TenantContext db)
{
    public async Task<ShopDetailsResult?> HandleAsync(CancellationToken ct = default)
    {
        Barbershop? shop = await db.Barbershops
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .FirstOrDefaultAsync(ct);
        return shop is null ? null : ShopDetailsResult.From(shop);
    }
}
