using Barber.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barber.DataAccess.Configurations;

public sealed class BarbershopConfiguration : IEntityTypeConfiguration<Barbershop>
{
    public void Configure(EntityTypeBuilder<Barbershop> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.Phone).HasMaxLength(32);
        builder.Property(x => x.TimeZone).IsRequired().HasMaxLength(64);
    }
}
