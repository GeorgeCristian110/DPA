using DPA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DPA.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<DpaDbContext>
            (options => options.UseNpgsql(connectionString));
        
        return services;
    }
}