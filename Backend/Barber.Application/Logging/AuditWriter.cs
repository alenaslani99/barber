using Barber.DataAccess;
using Barber.Domain;
using Microsoft.AspNetCore.Http;

namespace Barber.Application.Logging;

public sealed class AuditWriter(TenantContext db, IHttpContextAccessor http)
{
    /// <summary>Actor recorded for changes made through the local admin app.</summary>
    public const string AdminActor = "admin";

    public async Task WriteAsync(
        string action,
        string email,
        Guid? userId = null,
        bool success = true,
        string? failureReason = null,
        string? entityType = null,
        Guid? entityId = null,
        CancellationToken ct = default)
    {
        try
        {
            string? ip = http.HttpContext?.Connection.RemoteIpAddress?.ToString();
            db.AuditLogs.Add(new AuditLog
            {
                Action = action,
                Email = email,
                UserId = userId,
                Success = success,
                FailureReason = failureReason,
                EntityType = entityType,
                EntityId = entityId,
                IpAddress = ip
            });
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            // Audit must never break the request.
        }
    }
}
