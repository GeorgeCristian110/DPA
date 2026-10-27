using DPA.Domain.Cargo;
using DPA.Domain.Destinations;
using DPA.Domain.Manifests;
using DPA.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace DPA.Infrastructure.Persistence;

public class DpaDbContext : DbContext
{
    public DpaDbContext(DbContextOptions<DpaDbContext> options) : base(options) 
    {
    }

    public DbSet<Manifest> Manifests => Set<Manifest>();
    public DbSet<CargoItem> CargoItems => Set<CargoItem>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleProfile> VehicleProfiles => Set<VehicleProfile>();
    public DbSet<Destination> Destinations => Set<Destination>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DpaDbContext).Assembly);
    }
       
}