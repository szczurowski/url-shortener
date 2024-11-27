using UrlShortener.Api.Extensions;
using UrlShortener.Api.Services;
using UrlShortener.Application.Extensions;
using UrlShortener.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddExceptionHandler<ExceptionHandler>()
    .AddApiServices()
    .AddApplicationServices()
    .AddInfrastructureServices()
    .RunMigrations();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler("/error");
// app.UseExceptionHandler(new ExceptionHandlerOptions()
// {
//      
// })

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();