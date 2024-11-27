using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ShortUrlController(IShortUrlService service) : ControllerBase
{
    // GET
    [HttpGet("{shortUrl:alpha}")]
    public IActionResult Index(string shortUrl)
    {
        var url = service.GetShortUrl(shortUrl);

        return Ok(url);
    }
}