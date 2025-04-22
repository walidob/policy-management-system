using Finbuckle.MultiTenant;
using PolicyManagement.Application.Extensions;
using PolicyManagement.Infrastructure.DbContexts.DefaultDb.Initialization;
using PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts.Initialization;
using PolicyManagement.Infrastructure.Extensions;
using Scalar.AspNetCore;
using Serilog;
using PolicyManagementApp.Api.Extensions;
using PolicyManagementApp.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogConfiguration();

Log.Information("API Starting Up.");

builder.Services.AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
     .AddApiRateLimiting()
    .AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });

// TODO: API Versioning will be implemented in future versions
// TODO: HATEOAS will be implemented in future versions

builder.Services.AddControllers();
builder.Services.AddMemoryCache(); 
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddResponseCaching();

builder.Services.AddOpenApi();

builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration["CorsSettings:AllowedOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            ?? ["https://domain1.com"];

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

Log.Information("Application built.");

await app.Services.SeedDefaultDbDataAsync();
await app.Services.ApplyTenantsDbsMigrationsAsync();
await app.Services.SeedTenantsDbsDataAsync();

Log.Information("Configuring middleware pipeline.");

app.UseExceptionHandler();
app.UseResponseCompression();
app.UseResponseCaching();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseCors("ProductionPolicy");
}
else
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseSerilogRequestLogging();
}

app.UseDefaultFiles();
app.MapStaticAssets();
app.UseHttpsRedirection();
app.UseStatusCodePages();

app.UseCookiePolicy();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMultiTenant();

app.MapControllers();

app.MapHealthChecks("/health");

Log.Information("Application running.");

app.Run();
