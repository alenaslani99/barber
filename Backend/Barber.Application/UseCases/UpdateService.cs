using Barber.Application.Logging;
using Barber.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record UpdateServiceCommand(
    Guid Id,
    string? Name,
    decimal? Price,
    int? DurationMinutes,
    int? SlotMinutes,
    bool? IsActive,
    Guid OwnerId,
    string OwnerEmail);

public sealed class UpdateServiceHandler(TenantContext db, AuditWriter audit)
{
    public async Task<ServiceDetailResult?> HandleAsync(UpdateServiceCommand command, CancellationToken ct = default)
    {
        Domain.Service? service = await db.Services.FirstOrDefaultAsync(s => s.Id == command.Id, ct);
        if (service is null)
            return null;

        if (command.Name is not null)
            service.Name = command.Name;
        if (command.Price.HasValue)
            service.Price = command.Price.Value;
        if (command.DurationMinutes.HasValue)
            service.DurationMinutes = command.DurationMinutes.Value;
        if (command.SlotMinutes.HasValue)
            service.SlotMinutes = command.SlotMinutes.Value;
        if (command.IsActive.HasValue)
            service.IsActive = command.IsActive.Value;

        await db.SaveChangesAsync(ct);
        await audit.WriteAsync("service.update", command.OwnerEmail, command.OwnerId, true, null, "Service", service.Id, ct);
        return new ServiceDetailResult(service.Id, service.Name, service.Price, service.DurationMinutes, service.SlotMinutes, service.IsActive);
    }
}
