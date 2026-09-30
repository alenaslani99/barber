using Barber.Application.Exceptions;
using Barber.Application.Logging;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record CreateShopServiceCommand(string Name, decimal Price, int DurationMinutes, int SlotMinutes);

/// <summary>
/// Admin-side counterpart of <see cref="CreateServiceHandler"/>: resolves the tenant's
/// single shop itself and audits as the admin instead of a signed-in owner.
/// </summary>
public sealed class CreateShopServiceHandler(TenantContext db, AuditWriter audit)
{
    public async Task<ServiceDetailResult> HandleAsync(CreateShopServiceCommand command, CancellationToken ct = default)
    {
        Guid? shopId = await db.Barbershops
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(ct);
        if (shopId is null)
            throw new ConflictException("Create the shop (step 2) before adding services.");

        bool nameTaken = await db.Services
            .AnyAsync(s => s.BarbershopId == shopId && s.Name.ToLower() == command.Name.ToLower(), ct);
        if (nameTaken)
            throw new ConflictException($"Service '{command.Name}' already exists.");

        Service service = new()
        {
            BarbershopId = shopId.Value,
            Name = command.Name,
            Price = command.Price,
            DurationMinutes = command.DurationMinutes,
            SlotMinutes = command.SlotMinutes,
            IsActive = true
        };
        db.Services.Add(service);
        await db.SaveChangesAsync(ct);

        await audit.WriteAsync("admin.service.create", AuditWriter.AdminActor, null, true, null, "Service", service.Id, ct);
        return new ServiceDetailResult(
            service.Id, service.Name, service.Price, service.DurationMinutes, service.SlotMinutes, service.IsActive);
    }
}
