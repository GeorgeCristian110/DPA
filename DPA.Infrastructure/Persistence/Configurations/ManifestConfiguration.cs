using DPA.Domain.Manifests;
using DPA.Domain.Destinations;
using DPA.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPA.Infrastructure.Persistence.Configurations;

public class ManifestConfiguration : IEntityTypeConfiguration<Manifest>
{
    public void Configure(EntityTypeBuilder<Manifest> builder)
    {
        builder.ToTable("Manifests");
        
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.ConfirmedByName).HasMaxLength(50);
        builder.Property(m => m.ApprovedByName).HasMaxLength(50);

        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.HasOne<Destination>()
            .WithMany()
            .HasForeignKey(m => m.DestinationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        builder.HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(m => m.VehicleId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.OwnsMany(m => m.Placements, placement =>
        {
            placement.ToTable("Placements");
            placement.WithOwner().HasForeignKey("ManifestId");

            placement.HasKey(p => p.Id);
            placement.Property(p => p.Id).ValueGeneratedNever();

            placement.Property(p => p.Xm).HasPrecision(4, 2);
            placement.Property(p => p.Ym).HasPrecision(4, 2);
            placement.Property(p => p.LengthM).HasPrecision(4, 2);
            placement.Property(p => p.WidthM).HasPrecision(4, 2);
            placement.Property(p => p.HeightM).HasPrecision(4, 2);
            placement.Property(p => p.WeightKg).HasPrecision(5, 0);

        });
        
        builder.Navigation(m => m.Placements)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(m => m.Rejections, rejection =>
        {
            rejection.ToTable("Rejections");
            rejection.WithOwner().HasForeignKey("ManifestId");
            
            rejection.HasKey(r => r.Id);
            rejection.Property(r => r.Id).ValueGeneratedNever();

            rejection.Property(r => r.RejectedByUserName).HasMaxLength(50).IsRequired();
            rejection.Property(r => r.RejectionMessage).HasMaxLength(1000).IsRequired();
            
            
        });
        
        builder.Navigation(m => m.Rejections)
            .UsePropertyAccessMode(PropertyAccessMode.Field);



    }
}