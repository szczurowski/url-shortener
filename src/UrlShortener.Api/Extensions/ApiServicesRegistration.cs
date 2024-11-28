using Microsoft.Extensions.Internal;
using UrlShortener.Api.Services;
using UrlShortener.Infrastructure;

namespace UrlShortener.Api.Extensions;

public static class ApiServicesRegistration
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddSingleton<ISystemClock, SystemClock>()
            .AddSingleton<IApplicationConfiguration, ApplicationConfiguration>();
        
        return services;
    }
}