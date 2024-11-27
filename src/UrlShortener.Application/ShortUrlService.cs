using LanguageExt;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using UrlShortener.Domain.Contracts;
using UrlShortener.Domain.Models;
using UrlShortener.Domain.OperationErrors;

namespace UrlShortener.Application;

public interface IShortUrlService
{
    Task<Either<RetrieveError, string>> GetById(string id);
    Task<bool> DeleteById(string id);
    Task<Either<CreateError, CreateResponse>> Create(string id, CreateRequest request);
    Task<CreateResponse> Create(CreateRequest request);
}

public class ShortUrlService(
    ISystemClock systemClock,
    ILogger<ShortUrlService> logger,
    IRepository repository,
    IUrlShortenerGenerator generator) : IShortUrlService
{
    public async Task<Either<RetrieveError, string>> GetById(string id)
    {
        logger.LogInformation("GetById called");
        
        var entity = await repository.GetById(id);

        var result = entity == null 
            ? Either<RetrieveError, string>.Left(RetrieveError.NotFound) 
            : IsInvalid(entity)
                ? Either<RetrieveError, string>.Left(RetrieveError.Gone)
                : Either<RetrieveError, string>.Right(entity.Url);

        return result;
    }

    public async Task<bool> DeleteById(string id)
    {
        logger.LogInformation("DeleteById called");

        var entity = await repository.GetById(id);
        
        await repository.Delete(entity);

        return true;
    }

    public async Task<Either<CreateError, CreateResponse>> Create(string id, CreateRequest request)
    {
        logger.LogInformation("Create() called");

        var entity = await repository.GetById(id);
        if (entity != null)
        {
            var error = IsInvalid(entity)
                ? CreateError.AlreadyExistsInvalid
                : CreateError.AlreadyExists;
            
            return Either<CreateError, CreateResponse>.Left(error);
        }

        entity = new ShortUrl
        {
            Id = id,
            CreatedAt = systemClock.UtcNow,
            Url = request.Url,
            TtlMinutes = request.TtlMinutes,
        };
        await repository.Create(entity);
        var result = Map(entity);

        return Either<CreateError, CreateResponse>.Right(result);
    }
    
    private static CreateResponse Map(ShortUrl e) => 
        new (e.Id, e.Url, e.TtlMinutes, e.CreatedAt);

    public Task<CreateResponse> Create(CreateRequest request)
    {
        logger.LogInformation("Create() called");

        var id = generator.Generate(request.Url);
        
        return Task.FromResult(new CreateResponse(id, request.Url, request.TtlMinutes, systemClock.UtcNow));
    }

    private bool IsInvalid(ShortUrl entity)
    {
        if (entity.TtlMinutes == null)
        {
            return false;
        }
        
        var createdMinutesAgo = systemClock.UtcNow.Subtract(entity.CreatedAt).TotalMinutes;
        
        return createdMinutesAgo > entity.TtlMinutes.Value;
    }
}