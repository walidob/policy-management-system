using Microsoft.AspNetCore.Diagnostics;
using PolicyManagementApp.Api.Models.ApiModels;
using Serilog;
using System.Diagnostics;
using System.Text.Json;

namespace PolicyManagementApp.Api.Middleware;

public static class GlobalExceptionHandlingMiddleware
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

        if (environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler(appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";

                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                    var exception = exceptionHandlerFeature?.Error;

                    if (exception != null)
                    {
                        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
                        
                        var errorResponse = new ErrorResponseModel
                        {
                            Message = "An unexpected error occurred.",
                            TraceId = traceId,
                            StatusCode = context.Response.StatusCode
                        };

                        if (environment.IsDevelopment() || environment.IsStaging())
                        {
                            errorResponse.StackTrace = exception.ToString();
                        }

                        Log.Error(exception, "Unhandled exception occurred. TraceId: {TraceId}", traceId);

                        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, jsonOptions));
                    }
                });
            });
        }

        return app;
    }
} 