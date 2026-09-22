using Barber.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barber.DataAccess.Configurations;

public sealed class WorkingDayOverrideConfiguration : IEntityTypeConfiguration<WorkingDayOverride>
{
    public void Configure(EntityTypeBuilder<WorkingDayOverride> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(200);
        builder.HasOne<Barbershop>().WithMany().HasForeignKey(x => x.BarbershopId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Staff>().WithMany().HasForeignKey(x => x.StaffId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.BarbershopId, x.Date });
        builder.HasIndex(x => new { x.StaffId, x.Date });
    }
}
