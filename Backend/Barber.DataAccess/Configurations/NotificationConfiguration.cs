using Barber.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barber.DataAccess.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Recipient).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Subject).HasMaxLength(200);
        builder.Property(x => x.Body).IsRequired();
        builder.HasOne<Barbershop>().WithMany().HasForeignKey(x => x.BarbershopId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.Status, x.ScheduledAt });
    }
}
