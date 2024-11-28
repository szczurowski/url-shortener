using Serilog;
using UrlShortener.Api.Extensions;
using UrlShortener.Api.Services;
using UrlShortener.Application.Extensions;
using UrlShortener.Infrastructure.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services
    .AddSerilog(config =>
    {
        config.WriteTo.Console();
    })
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddExceptionHandler<ExceptionHandler>()
    .AddApiServices()
    .AddApplicationServices()
    .AddInfrastructureServices()
    .RunMigrations();

try
{
    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseExceptionHandler("/error");
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
