using Barber.DataAccess;
using Barber.Domain;
using Microsoft.EntityFrameworkCore;

namespace Barber.Application.UseCases;

public sealed record GetAvailabilityQuery(Guid StaffId, DateOnly Date, Guid? ServiceId);
public sealed record AvailabilityResult(
    DateOnly Date,
    bool Closed,
    string Open,
    string Close,
    int SlotMinutes,
    List<string> Taken);

public sealed class GetAvailabilityHandler(TenantContext db)
{
    private static TimeZoneInfo ShopTime()
    {
        foreach (string id in new[] { "Europe/Berlin", "W. Europe Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
        }
        return TimeZoneInfo.Utc;
    }

    public async Task<AvailabilityResult?> HandleAsync(GetAvailabilityQuery query, CancellationToken ct = default)
    {
        Staff? staff = await db.Staff
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.StaffId && s.IsActive, ct);
        if (staff is null)
            return null;

        int step = 30;
        if (query.ServiceId.HasValue)
        {
            Service? service = await db.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == query.ServiceId.Value && s.IsActive, ct);
            if (service is null)
                return null;
            step = service.SlotMinutes > 0 ? service.SlotMinutes : 30;
        }

        WorkingHours? hours = await db.WorkingHours
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.BarbershopId == staff.BarbershopId && h.Day == query.Date.DayOfWeek, ct);
        if (hours is null || hours.IsClosed)
            return new AvailabilityResult(query.Date, true, string.Empty, string.Empty, step, []);

        bool dayOff = await db.WorkingDayOverrides.AsNoTracking().AnyAsync(o =>
            o.BarbershopId == staff.BarbershopId &&
            o.Date == query.Date &&
            (o.StaffId == null || o.StaffId == staff.Id), ct);
        if (dayOff)
            return new AvailabilityResult(query.Date, true, hours.Open.ToString("HH:mm"), hours.Close.ToString("HH:mm"), step, []);

        TimeZoneInfo berlin = ShopTime();
        DateTime localMidnight = query.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        DateTimeOffset dayStart = new(localMidnight, berlin.GetUtcOffset(localMidnight));
        DateTimeOffset dayEnd = dayStart.AddDays(1);
        DateTimeOffset dayStartUtc = dayStart.ToUniversalTime();
        DateTimeOffset dayEndUtc = dayEnd.ToUniversalTime();

        List<Booking> bookings = await db.Bookings.AsNoTracking().Where(b =>
            b.StaffId == staff.Id &&
            b.Status != BookingStatus.Cancelled &&
            b.StartsAt < dayEndUtc &&
            b.StartsAt >= dayStartUtc).ToListAsync(ct);

        HashSet<string> taken = [];
        foreach (Booking booking in bookings)
        {
            DateTimeOffset slot = booking.StartsAt;
            while (slot < booking.EndsAt)
            {
                DateTime local = TimeZoneInfo.ConvertTime(slot, berlin).DateTime;
                taken.Add(new TimeOnly(local.Hour, local.Minute).ToString("HH:mm"));
                slot = slot.AddMinutes(step);
            }
        }

        List<string> ordered = taken.ToList();
        ordered.Sort(StringComparer.Ordinal);
        return new AvailabilityResult(
            query.Date,
            false,
            hours.Open.ToString("HH:mm"),
            hours.Close.ToString("HH:mm"),
            step,
            ordered);
    }
}
