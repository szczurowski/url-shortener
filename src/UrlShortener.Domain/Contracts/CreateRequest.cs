namespace UrlShortener.Domain.Contracts;

public record CreateRequest(string Url, int? TtlMinutes);