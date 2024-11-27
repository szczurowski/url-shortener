using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace UrlShortener.Infrastructure.Extensions;

public static class InfrastructureServicesRegistrations
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddDbContext<DatabaseContext>((serviceProvider, options) =>
            {
                var configuration = serviceProvider.GetRequiredService<IApplicationConfiguration>();
                
                // if fancy to move to a different provider one can change it here
                options.UseSqlServer(configuration.ConnectionString);
            });
        
        return services;
    }
    
    public static IServiceCollection RunMigrations(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var databaseContext = serviceProvider.GetRequiredService<DatabaseContext>();
        
        databaseContext.Database.Migrate();
        
        return services;
    }
}