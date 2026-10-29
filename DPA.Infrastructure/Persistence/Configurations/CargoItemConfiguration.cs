using DPA.Domain.Cargo;
using DPA.Domain.Manifests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPA.Infrastructure.Persistence.Configurations;

public class CargoItemConfiguration : IEntityTypeConfiguration<CargoItem>
{
    public void Configure(EntityTypeBuilder<CargoItem> builder)
    {
        builder.ToTable("CargoItems");
        
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        
        builder.Property(c => c.PalletCode)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.HasIndex(c => c.PalletCode).IsUnique();
        
        builder.Property(c => c.LengthM).HasPrecision(4, 2);
        builder.Property(c => c.WidthM).HasPrecision(4, 2);
        builder.Property(c => c.HeightM).HasPrecision(4, 2);
        builder.Property(c => c.WeightKg).HasPrecision(5, 0);
        
        builder.Property(c => c.Stability)
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.HasOne<Manifest>()
            .WithMany()
            .HasForeignKey(c => c.ManifestId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}