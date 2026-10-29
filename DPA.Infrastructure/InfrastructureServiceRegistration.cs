using DPA.Application.Manifests;
using DPA.Infrastructure.Persistence;
using DPA.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DPA.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<DpaDbContext>
            (options => options.UseNpgsql(connectionString));
        
        services.AddScoped<IManifestRepository, ManifestRepository>();
        
        return services;
    }
}