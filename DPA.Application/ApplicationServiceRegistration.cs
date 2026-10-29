using DPA.Application.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace DPA.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISender, Sender>();

        var assembly = typeof(ApplicationServiceRegistration).Assembly;

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            foreach (var implemented in type.GetInterfaces() )
            {
                if (implemented.IsGenericType &&
                    implemented.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                {
                    services.AddScoped(implemented, type);
                }
            }
        }
        
        return services;
    }
}