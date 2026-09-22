using Barber.Application.Logging;
using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record CreateServiceCommand(
    Guid BarbershopId,
    string Name,
    decimal Price,
    int DurationMinutes,
    int SlotMinutes,
    Guid OwnerId,
    string OwnerEmail);
public sealed record ServiceDetailResult(
    Guid Id,
    string Name,
    decimal Price,
    int DurationMinutes,
    int SlotMinutes,
    bool IsActive);

public sealed class CreateServiceHandler(TenantContext db, AuditWriter audit)
{
    public async Task<ServiceDetailResult> HandleAsync(CreateServiceCommand command, CancellationToken ct = default)
    {
        Barbershop? shop = await db.Barbershops
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == command.BarbershopId, ct);
        if (shop is null)
            throw new KeyNotFoundException($"Barbershop '{command.BarbershopId}' not found.");

        Service service = new()
        {
            BarbershopId = shop.Id,
            Name = command.Name,
            Price = command.Price,
            DurationMinutes = command.DurationMinutes,
            SlotMinutes = command.SlotMinutes,
            IsActive = true
        };
        db.Services.Add(service);
        await db.SaveChangesAsync(ct);
        await audit.WriteAsync("service.create", command.OwnerEmail, command.OwnerId, true, null, "Service", service.Id, ct);
        return new ServiceDetailResult(service.Id, service.Name, service.Price, service.DurationMinutes, service.SlotMinutes, service.IsActive);
    }
}
