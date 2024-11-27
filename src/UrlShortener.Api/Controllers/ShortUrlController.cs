using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application;
using UrlShortener.Domain.Contracts;
using UrlShortener.Domain.OperationErrors;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ShortUrlController(IShortUrlService service) : ControllerBase
{
    [HttpGet("{id:alpha}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await service.GetById(id);

        return result.Match(left => left switch
            {
                RetrieveError.Gone => StatusCode((int)HttpStatusCode.Gone),
                RetrieveError.NotFound => NotFound(),
                _ => throw new ArgumentOutOfRangeException(nameof(left), left, null)
            },
            right => (IActionResult) Ok(right));
    }
    
    [HttpPut("{id:alpha}")]
    public async Task<IActionResult> CreateWithId(
        [FromRoute, Required] string id,
        [FromBody, Required] CreateRequest request)
    {
        var result = await service.Create(id, request);
        
        return result.Match(left => left switch
            {
                CreateError.AlreadyExists => Conflict(),
                CreateError.AlreadyExistsInvalid => Conflict(),
                _ => throw new ArgumentOutOfRangeException(nameof(left), left, null)
            },
            right => (IActionResult) Created(right.Id, right));
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