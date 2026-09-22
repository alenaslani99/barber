using Barber.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barber.DataAccess.Configurations;

public sealed class WorkingHoursConfiguration : IEntityTypeConfiguration<WorkingHours>
{
    public void Configure(EntityTypeBuilder<WorkingHours> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasOne<Barbershop>().WithMany().HasForeignKey(x => x.BarbershopId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.BarbershopId, x.Day }).IsUnique();
    }
}
