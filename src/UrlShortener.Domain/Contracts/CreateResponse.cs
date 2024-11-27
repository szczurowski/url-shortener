namespace UrlShortener.Domain.Contracts;

public record CreateResponse(string Id, string Url, int? TtlMinutes, DateTimeOffset CreatedAt);