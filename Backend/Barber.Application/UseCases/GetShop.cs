using Barber.Application.UseCases;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record ShopHourItem(int Day, string Open, string Close, bool Closed);
public sealed record ShopResult(
    Guid Id,
    string Name,
    string Tagline,
    string Description,
    string? Address,
    string? Phone,
    List<ShopHourItem> Hours,
    List<BarberListItem> Staff,
    List<ServiceListItem> Services);

public sealed class GetShopHandler(
    TenantContext db,
    GetBarbersHandler barbers,
    GetServicesHandler services)
{
    public async Task<ShopResult?> HandleAsync(CancellationToken ct = default)
    {
        Domain.Barbershop? shop = await db.Barbershops
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .FirstOrDefaultAsync(ct);
        if (shop is null)
            return null;

        List<WorkingHours> hours = await db.WorkingHours
            .AsNoTracking()
            .Where(h => h.BarbershopId == shop.Id)
            .ToListAsync(ct);

        List<BarberListItem> staff = await barbers.HandleAsync(ct);
        List<ServiceListItem> serviceList = await services.HandleAsync(ct);

        List<ShopHourItem> hourItems = hours
            .OrderBy(h => h.Day)
            .Select(h => new ShopHourItem(
                (int)h.Day,
                h.Open.ToString("HH:mm"),
                h.Close.ToString("HH:mm"),
                h.IsClosed))
            .ToList();

        return new ShopResult(
            shop.Id,
            shop.Name,
            shop.Tagline,
            shop.Description,
            shop.Address,
            shop.Phone,
            hourItems,
            staff,
            serviceList);
    }
}
