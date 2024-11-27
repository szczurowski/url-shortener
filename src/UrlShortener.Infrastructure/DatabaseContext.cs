using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Models;

namespace UrlShortener.Infrastructure;

public class DatabaseContext : DbContext
{
    public DatabaseContext() { }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("sql_connection");
        }

        base.OnConfiguring(optionsBuilder);
    }

    public DbSet<ShortUrl> ShortUrls { get; set; }
}