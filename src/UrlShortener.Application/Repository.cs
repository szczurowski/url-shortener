using UrlShortener.Domain.Models;
using UrlShortener.Infrastructure;

namespace UrlShortener.Application;

public interface IRepository
{
    Task<ShortUrl?> GetById(string id);
    Task Delete(ShortUrl entity);
    Task Create(ShortUrl entity);
}

public class Repository(DatabaseContext context) : IRepository
{
    public async Task<ShortUrl?> GetById(string id)
    {
        var result = await context.ShortUrls.FindAsync(id);

        return result;
    }

    public async Task Delete(ShortUrl entity)
    {
        context.ShortUrls.Remove(entity);
        
        await context.SaveChangesAsync();
    }

    public async Task Create(ShortUrl entity)
    {
        context.ShortUrls.Add(entity);
        
        await context.SaveChangesAsync();
    }
}