namespace UrlShortener.Infrastructure;

public interface IApplicationConfiguration
{
    public string ConnectionString { get; }
}