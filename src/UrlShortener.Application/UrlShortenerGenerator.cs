using System.Security.Cryptography;
using System.Text;
using Base62;

namespace UrlShortener.Application;

public interface IUrlShortenerGenerator
{
    string Generate(string url);
}

public class UrlShortenerGenerator : IUrlShortenerGenerator
{
    public string Generate(string url)
    {
        // poor-man's implementation
        var inputBytes = Encoding.UTF8.GetBytes(url);
        var hashBytes = MD5.HashData(inputBytes);
        var hash = hashBytes[..5].ToBase62(); // alphanumeric only and 2^40 possible urls

        return hash;
    }
}