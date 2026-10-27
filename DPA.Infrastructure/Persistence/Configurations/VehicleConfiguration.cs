using DPA.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPA.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.Code).IsRequired().HasMaxLength(10);

        // Metres
        builder.Property(v => v.LengthM).HasPrecision(4, 2);
        builder.Property(v => v.WidthM).HasPrecision(4, 2);
        builder.Property(v => v.HeightM).HasPrecision(4, 2);
        builder.Property(v => v.FrontToRearSupportM).HasPrecision(4, 2);
        builder.Property(v => v.EmptyCgXm).HasPrecision(4, 2);
        builder.Property(v => v.FrontSupportOffsetM).HasPrecision(4, 2);
       

        // Kilograms
        builder.Property(v => v.EmptyWeightKg).HasPrecision(5, 0);
        builder.Property(v => v.MaxPayloadKg).HasPrecision(5, 0);

        builder.HasOne(v => v.Profile)
            .WithMany()
            .HasForeignKey("VehicleProfileId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(v => v.Code).IsUnique();
    }
}