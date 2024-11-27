using Microsoft.Extensions.DependencyInjection;

namespace UrlShortener.Application.Extensions;

public static class ApplicationServicesRegistration
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<IShortUrlService, ShortUrlService>();
        
        return services;
    }
}