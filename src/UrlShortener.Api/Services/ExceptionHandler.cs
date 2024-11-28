using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using UrlShortener.Domain.Contracts;

namespace UrlShortener.Api.Services;

public class ExceptionHandler(ILogger<ExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, exception.Message);
        var response = new ErrorResponse("Unhandled exception occured. Contact the support team");
        
        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        
        return true;
    }
}