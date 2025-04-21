using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;

namespace PolicyManagementApp.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IWebHostEnvironment _environment;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        IWebHostEnvironment environment,
        IProblemDetailsService problemDetailsService)
    {
        _environment = environment;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        
        int statusCode = exception switch
        {
            ArgumentException or ArgumentNullException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = "An error occurred",
            Type = exception.GetType().Name,
            Instance = httpContext.Request.Path,
            Detail = _environment.IsDevelopment() 
                ? exception.Message 
                : "An error occurred processing your request."
        };

        problemDetails.Extensions["traceId"] = traceId;
        
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.ToString();
        }
        
        Log.Error(exception, "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, StatusCode: {StatusCode}",
            traceId, httpContext.Request.Path, statusCode);
        
        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }
} 