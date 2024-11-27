using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Domain.Models;

public class ShortUrl
{
    [Key]
    public required string Id { get; set; }
    public required string Url { get; set; }
    public int? Ttl { get; set; }
}