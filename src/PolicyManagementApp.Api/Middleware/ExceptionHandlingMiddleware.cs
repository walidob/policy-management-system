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
        Log.Error(exception, "Unhandled exception: {Path}", context.Request.Path.Value);

        var statusCode = StatusCodes.Status500InternalServerError;
        var message = "An internal server error occurred.";

        // Custom exception handling based on exception type
        switch (exception)
        {
            case InvalidOperationException:
                statusCode = StatusCodes.Status400BadRequest;
                message = exception.Message;
                break;
            case UnauthorizedAccessException:
                statusCode = StatusCodes.Status401Unauthorized;
                message = "Unauthorized access.";
                break;
            case KeyNotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                message = "Resource not found.";
                break;
        }

        var errorResponse = new ErrorResponseModel
        {
            Error = message,
            StatusCode = statusCode
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = errorResponse.StatusCode;
        
        return context.Response.WriteAsJsonAsync(errorResponse);
    }
}
