using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using PolicyManagement.Persistence.Extensions;
using PolicyManagementApp.Api.Middleware;
using PolicyManagementApp.Api.Models.ApiModels;
using Scalar.AspNetCore;
using Serilog;

namespace PolicyManagementApp.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configure logger temporarily for startup issues, reading minimal config
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            Log.Information("API Starting Up.");

            var builder = WebApplication.CreateBuilder(args);

            // --- Full Logging Setup --- 
            // Replace bootstrap logger with full config from appsettings.json
            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration) // Reads sinks (Console, File) from appsettings
                .ReadFrom.Services(services) // Allows DI injection into sinks
                .Enrich.FromLogContext());

            Log.Debug("Registering services.");
            //builder.Services.AddApplicationServices(); 
            //builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddPersistenceServices(builder.Configuration);

            // API Layer Services
            builder.Services.AddControllers();
            builder.Services.AddOpenApi(); // Swagger/OpenAPI

            Log.Information("Services registered.");

            var app = builder.Build();
            Log.Information("Application built.");


            Log.Information("Configuring middleware pipeline.");
            app.UseDefaultFiles();
            app.MapStaticAssets();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.MapOpenApi();
                app.MapScalarApiReference();
                app.UseSerilogRequestLogging(); // Log HTTP Requests
            }
            else
            {
                app.UseMiddleware<ExceptionHandlingMiddleware>();
                app.UseHsts();
            }

            // Use our custom status code pages middleware
            app.UseStatusCodePages();

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();

            app.MapFallbackToFile("/index.html");
            Log.Information("Middleware pipeline configured.");

            Log.Information("Running application.");
            app.Run();
        }
    }
}
