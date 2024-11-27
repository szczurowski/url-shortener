using Microsoft.Extensions.Logging;

namespace UrlShortener.Application;

public interface IShortUrlService
{
    string GetShortUrl(string shortUrl);
}

public class ShortUrlService(ILogger<ShortUrlService> logger) : IShortUrlService
{
    public string GetShortUrl(string shortUrl)
    {
        logger.LogInformation("GetShortUrl called");
        
        return Guid.NewGuid().ToString();
    }
}