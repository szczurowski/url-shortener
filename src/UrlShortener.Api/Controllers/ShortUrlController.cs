using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application;
using UrlShortener.Domain.Contracts;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ShortUrlController(IShortUrlService service) : ControllerBase
{
    [HttpGet("{id:alpha}")]
    public async Task<IActionResult> GetById(string id)
    {
        var url = await service.GetById(id);

        return string.IsNullOrEmpty(url) 
            ? NotFound() 
            : Redirect(url);
    }
    
    [HttpPut("{id:alpha}")]
    public async Task<IActionResult> CreateWithId(
        [FromRoute, Required] string id,
        [FromBody, Required] CreateRequest request)
    {
        var url = await service.Create(id, request);
        
        return Created("url", url);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequest request)
    {
        var url = await service.Create(request);
        
        return Created("url", url);
    }
    
    [HttpDelete("{id:alpha}")]
    public async Task<IActionResult> DeleteById(string id)
    {
        var found = await service.DeleteById(id);

        return found 
            ? NoContent() 
            : NotFound();
    }
}