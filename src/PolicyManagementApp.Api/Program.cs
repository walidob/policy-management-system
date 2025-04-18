using PolicyManagement.Application.Extensions;
using PolicyManagement.Persistence.Extensions;
using PolicyManagement.Persistence.Initialization;
using PolicyManagementApp.Api.Middleware;
using Scalar.AspNetCore;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogConfiguration();

Log.Information("API Starting Up.");
builder.Services.AddApplicationServices();
//builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddPersistenceServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi(); // Swagger/OpenAPI

Log.Information("Services registered.");

var app = builder.Build();
Log.Information("Application built.");

Log.Information("Checking if demo data seeding is enabled...");
await app.Services.SeedDataAsync();

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
