using Barber.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barber.DataAccess.Configurations;

public sealed class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasOne<Barbershop>().WithMany().HasForeignKey(x => x.BarbershopId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Seniority>().WithMany().HasForeignKey(x => x.SeniorityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.BarbershopId, x.UserId }).IsUnique();
    }
}
