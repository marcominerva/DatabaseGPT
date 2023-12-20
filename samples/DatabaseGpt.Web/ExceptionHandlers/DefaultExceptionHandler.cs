using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseGpt.Web.ExceptionHandlers;

public class DefaultExceptionHandler(IProblemDetailsService problemDetailsService, IWebHostEnvironment webHostEnvironment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails
        {
            Status = httpContext.Response.StatusCode,
            Title = exception.GetType().FullName,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        if (webHostEnvironment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        await problemDetailsService.WriteAsync(new()
        {
            HttpContext = httpContext,
            AdditionalMetadata = httpContext.Features.Get<IExceptionHandlerFeature>()?.Endpoint?.Metadata,
            ProblemDetails = problemDetails,
            Exception = exception
        });

        return true;
    }
}
