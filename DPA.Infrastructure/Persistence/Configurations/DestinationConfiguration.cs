using DPA.Domain.Destinations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DPA.Infrastructure.Persistence.Configurations;

public class DestinationConfiguration : IEntityTypeConfiguration<Destination>
{
    public void Configure(EntityTypeBuilder<Destination> builder)
    {
        builder.ToTable("Destinations");
        
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();
        
        builder.Property(d => d.Name).IsRequired().HasMaxLength(120);
        builder.Property(d => d.Address ).IsRequired().HasMaxLength(120);
        builder.Property(d => d.ZipCode).IsRequired().HasMaxLength(10);
        builder.Property(d => d.City).IsRequired().HasMaxLength(60);
    }
}