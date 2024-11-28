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
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await service.GetById(id);
        
        return result.Match(Redirect, MapError);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> CreateWithId(
        [FromRoute, Required] string id,
        [FromBody, Required] CreateRequest request)
    {
        var result = await service.Create(id, request);

        return result.Match(
            right => Created(Url.ActionLink(action: nameof(GetById), values: new { id }), right), 
            MapError);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequest request)
    {
        var result = await service.Create(request);
        
        return result.Match(
            right => Created(Url.ActionLink(action: nameof(GetById), values: new { right.Id }), right), 
            MapError);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteById(string id)
    {
        var result = await service.DeleteById(id);

        return result.Match(_ => NoContent(), MapError);
    }

    private IActionResult MapError(RetrieveError error) => error switch
    {
        RetrieveError.Gone => StatusCode((int)HttpStatusCode.Gone, new ErrorResponse("Entry is invalid")),
        RetrieveError.NotFound => NotFound(new ErrorResponse("Entry not found")),
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null) // logs and returns generic error
    };
    
    private IActionResult MapError(CreateError error) => error switch
    {
        CreateError.MissingUrl => BadRequest(new ErrorResponse("Url is missing.")),
        CreateError.BadUrl => BadRequest(new ErrorResponse("Url is badBadly formatted url.")),
        CreateError.AlreadyExists => Conflict(new ErrorResponse("Id already exists.")),
        CreateError.AlreadyExistsInvalid => Conflict(new ErrorResponse("Id already exists. Points to invalid entry.")),
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null) // logs and returns generic error
    };
    
    private IActionResult MapError(DeleteError error) => error switch
    {
        DeleteError.NotFound => NotFound(new ErrorResponse("Entry not found")),
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null) // logs and returns generic error
    };
}