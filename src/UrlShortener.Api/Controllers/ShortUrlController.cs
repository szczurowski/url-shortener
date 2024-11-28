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

        return result.Match(MapError, Ok);
    }

    [HttpPut("{id:alpha}")]
    public async Task<IActionResult> CreateWithId(
        [FromRoute, Required] string id,
        [FromBody, Required] CreateRequest request)
    {
        var result = await service.Create(id, request);

        return result.Match(MapError, right => Created(right.Id, right));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequest request)
    {
        var result = await service.Create(request);

        return result.Match(MapError, right => Created(right.Id, right));
    }

    [HttpDelete("{id:alpha}")]
    public async Task<IActionResult> DeleteById(string id)
    {
        var result = await service.DeleteById(id);

        return result.Match(MapError, _ => NoContent());
    }

    private IActionResult MapError(RetrieveError error) => error switch
    {
        RetrieveError.Gone => StatusCode((int)HttpStatusCode.Gone),
        RetrieveError.NotFound => NotFound(),
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };
    
    private IActionResult MapError(CreateError error) => error switch
    {
        CreateError.MissingUrl => BadRequest(),
        CreateError.BadUrl => BadRequest(),
        CreateError.AlreadyExists => Conflict(),
        CreateError.AlreadyExistsInvalid => Conflict(),
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };
    
    private IActionResult MapError(DeleteError error) => error switch
    {
        DeleteError.NotFound => NotFound(),
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };
}