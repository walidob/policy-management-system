using Scalar.AspNetCore;
using Serilog;
using Microsoft.AspNetCore.Diagnostics;
using PolicyManagementApp.Api.Filters;
using PolicyManagementApp.Api.Models.ApiModels;
using System;

namespace PolicyManagementApp.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .Build();

            // Configure Serilog logger
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            Log.Information("Starting Policy Management API");

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog();

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            });

            builder.Services.AddOpenApi();

            var app = builder.Build();

            app.UseDefaultFiles();
            app.MapStaticAssets();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.MapOpenApi();
                app.MapScalarApiReference();
            }
            else
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = "application/json";

                        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                        var exception = exceptionHandlerPathFeature?.Error;

                        Log.Error(exception, "Unhandled exception occurred");

                        var errorResponse = new ErrorResponseModel
                        {
                            Error = "An error occurred while processing your request.",
                            StatusCode = context.Response.StatusCode
                        };

                        await context.Response.WriteAsJsonAsync(errorResponse);
                    });
                });

                app.UseHsts();
            }

            app.UseStatusCodePages(async statusCodeContext =>
            {
                var context = statusCodeContext.HttpContext;
                context.Response.ContentType = "application/json";
                
                var errorResponse = new ErrorResponseModel
                {
                    Error = $"Status code error: {context.Response.StatusCode}",
                    StatusCode = context.Response.StatusCode
                };
                
                await context.Response.WriteAsJsonAsync(errorResponse);
                
                Log.Error("Status code {StatusCode} returned for {Path}", 
                    context.Response.StatusCode, 
                    context.Request.Path);
            });

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
