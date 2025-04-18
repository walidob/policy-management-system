using PolicyManagementApp.Api.Models.ApiModels;
using Serilog;

namespace PolicyManagementApp.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log the exception details
        Log.Error(exception, 
            "Unhandled exception: {Path}", 
            context.Request.Path.Value);

        var errorResponse = new ErrorResponseModel
        {
            Error = "An internal server error occurred.",
            StatusCode = StatusCodes.Status500InternalServerError
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = errorResponse.StatusCode;
        
        return context.Response.WriteAsJsonAsync(errorResponse);
    }
}
