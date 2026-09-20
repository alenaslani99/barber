using Barber.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barber.DataAccess.Configurations;

public sealed class SeniorityConfiguration : IEntityTypeConfiguration<Seniority>
{
    public static readonly Guid JuniorId = new("11111111-1111-1111-1111-111111111111");
    public static readonly Guid BarberId = new("22222222-2222-2222-2222-222222222222");
    public static readonly Guid SeniorId = new("33333333-3333-3333-3333-333333333333");
    public static readonly Guid MasterId = new("44444444-4444-4444-4444-444444444444");

    public void Configure(EntityTypeBuilder<Seniority> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasData(
            new Seniority { Id = JuniorId, Name = "Junior", Level = 0 },
            new Seniority { Id = BarberId, Name = "Barber", Level = 1 },
            new Seniority { Id = SeniorId, Name = "Senior", Level = 2 },
            new Seniority { Id = MasterId, Name = "Master", Level = 3 });
    }
}
