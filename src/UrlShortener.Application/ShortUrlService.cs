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
    Task<Either<DeleteError, Unit>> DeleteById(string id);
    Task<Either<CreateError, CreateResponse>> Create(string id, CreateRequest request);
    Task<Either<CreateError, CreateResponse>> Create(CreateRequest request);
}

public class ShortUrlService(
    ISystemClock systemClock,
    ILogger<ShortUrlService> logger,
    IRepository repository,
    IUrlShortenerGenerator generator) : IShortUrlService
{
    public async Task<Either<RetrieveError, string>> GetById(string id)
    {
        var entity = await repository.GetById(id);

        if (entity == null)
        {
            logger.LogInformation("Entity not found, Id:{Id}", id);
            return Prelude.Left(RetrieveError.NotFound);
        }

        if (IsInvalid(entity))
        {
            logger.LogInformation("Entity invalid, Id:{Id}", id);
            return Prelude.Left(RetrieveError.Gone);
        }
        
        return Prelude.Right(entity.Url);
    }

    public async Task<Either<DeleteError, Unit>> DeleteById(string id)
    {
        var entity = await repository.GetById(id);
        if (entity == null)
        {
            logger.LogInformation("Entity not found, Id:{Id}", id);

            return Prelude.Left(DeleteError.NotFound);
        }
        
        await repository.Delete(entity);

        return Prelude.Right(Unit.Default);
    }

    public async Task<Either<CreateError, CreateResponse>> Create(string id, CreateRequest request)
    {
        var validationError = ValidateCreate(request);
        if (validationError != null)
        {
            return Prelude.Left(validationError.Value);
        }
        
        var entity = await repository.GetById(id);
        if (entity != null)
        {
            logger.LogInformation("Tried to override existing entity, Id:{Id}", id);
            var error = IsInvalid(entity)
                ? CreateError.AlreadyExistsInvalid
                : CreateError.AlreadyExists;
            
            return Prelude.Left(error);
        }

        var result = await CreateInternal(id, request);

        return Prelude.Right(result);
    }

    public async Task<Either<CreateError, CreateResponse>> Create(CreateRequest request)
    {
        var validationError = ValidateCreate(request);
        if (validationError != null)
        {
            return Prelude.Left(validationError.Value);
        }

        var id = generator.Generate(request.Url!);
        var entity = await repository.GetById(id);
        if (entity != null)
        {
            // TODO: improvement: come up with a way of handling collisions
            logger.LogInformation("Generated Id:{Id} that matches existing entity", id);
            
            return Prelude.Left(CreateError.AlreadyExists);
        }
        
        var result = await CreateInternal(id, request);

        return Prelude.Right(result);
    }
    
    private static CreateResponse Map(ShortUrl e) => 
        new (e.Id, e.Url, e.TtlMinutes, e.CreatedAt);

    private async Task<CreateResponse> CreateInternal(string id, CreateRequest request)
    {
        var entity = new ShortUrl
        {
            Id = id,
            CreatedAt = systemClock.UtcNow,
            Url = request.Url!,
            TtlMinutes = request.TtlMinutes,
        };
        await repository.Create(entity);
        var result = Map(entity);
        
        return result;
    }

    private CreateError? ValidateCreate(CreateRequest request)
    {
        if (string.IsNullOrEmpty(request.Url))
        {
            logger.LogInformation("Url is missing");
            return CreateError.MissingUrl;
        }
        
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
        {
            logger.LogInformation("Badly formatted url: {Url}", request.Url);
            return CreateError.BadUrl;
        }
        
        return null;
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