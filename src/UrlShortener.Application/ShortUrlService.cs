using Microsoft.Extensions.Logging;
using UrlShortener.Domain.Contracts;
using UrlShortener.Domain.Models;

namespace UrlShortener.Application;

public interface IShortUrlService
{
    Task<string?> GetById(string id);
    Task<bool> DeleteById(string id);
    Task<CreateResponse> Create(string id, CreateRequest request);
    Task<CreateResponse> Create(CreateRequest request);
}

public class ShortUrlService(
    ILogger<ShortUrlService> logger,
    IRepository repository,
    IUrlShortenerGenerator generator) : IShortUrlService
{
    public async Task<string?> GetById(string id)
    {
        logger.LogInformation("GetById called");
        
        var entity = await repository.GetById(id);
        
        return entity?.Url;
    }

    public async Task<bool> DeleteById(string id)
    {
        logger.LogInformation("DeleteById called");

        var entity = await repository.GetById(id);
        if (entity == null)
        {
            return false;
        }
        
        await repository.Delete(entity);

        return true;
    }

    public Task<CreateResponse> Create(string id, CreateRequest request)
    {
        logger.LogInformation("Create() called");
        
        // check existence of 'Id'
        
        return Task.FromResult(new CreateResponse(id, request.Url, request.TtlMinutes));
    }

    public Task<CreateResponse> Create(CreateRequest request)
    {
        logger.LogInformation("Create() called");

        var id = generator.Generate(request.Url);
        
        return Task.FromResult(new CreateResponse(id, request.Url, request.TtlMinutes));
    }
}