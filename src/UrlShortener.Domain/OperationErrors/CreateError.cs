namespace UrlShortener.Domain.OperationErrors;

public enum CreateError
{
    MissingUrl,
    BadUrl,
    AlreadyExists,
    AlreadyExistsInvalid
}