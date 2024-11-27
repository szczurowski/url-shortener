namespace UrlShortener.Application;

public interface IUrlShortenerGenerator
{
    string Generate(string url);
}

public class UrlShortenerGenerator : IUrlShortenerGenerator
{
    public string Generate(string url)
    {
        // TODO: implement
        
        // quick implementation for now
        var uri = new Uri(url);
        return uri.AbsolutePath[..1];
    }
}