using DPA.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPA.Infrastructure.Persistence.Configurations;

public class VehicleProfileConfiguration : IEntityTypeConfiguration<VehicleProfile>
{
    public void Configure(EntityTypeBuilder<VehicleProfile> builder)
    {
        builder.ToTable("VehicleProfiles");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        
        builder.Property(p => p.Name).IsRequired().HasMaxLength(120);
        builder.Property(p => p.MinFrontOffsetM).HasPrecision(3,2);
        builder.Property(p => p.MaxFrontOffsetM).HasPrecision(3,2);

    }
}