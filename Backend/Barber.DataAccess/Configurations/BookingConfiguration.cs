using Barber.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barber.DataAccess.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasOne<Barbershop>().WithMany().HasForeignKey(x => x.BarbershopId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Service>().WithMany().HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Staff>().WithMany().HasForeignKey(x => x.StaffId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.BarbershopId, x.StartsAt });
        builder.HasIndex(x => new { x.StaffId, x.StartsAt });
    }
}
