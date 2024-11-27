using UrlShortener.Infrastructure;

namespace UrlShortener.Api.Services;

public class ApplicationConfiguration(IConfiguration configuration) : IApplicationConfiguration
{
    public string ConnectionString { get; } = configuration.GetConnectionString("DefaultConnection") ??
                                              throw new NullReferenceException("Default connection string not found");
}