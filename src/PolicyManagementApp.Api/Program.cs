using Finbuckle.MultiTenant;
using PolicyManagement.Application.Extensions;
using PolicyManagement.Infrastructure.DbContexts.DefaultDb.Initialization;
using PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts.Initialization;
using PolicyManagement.Infrastructure.Extensions;
using PolicyManagementApp.Api.Middleware;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilogConfiguration();

Log.Information("API Starting Up.");

builder.Services.AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddResponseCaching();

builder.Services.AddControllers();
builder.Services.AddMemoryCache(); 
builder.Services.AddOpenApi();

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

app.UseDefaultFiles();
app.MapStaticAssets();
app.UseHttpsRedirection();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    // In dev, we use Angular proxy so CORS isn't needed
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseSerilogRequestLogging();
}
else
{
    app.UseHsts();
    app.UseCors("ProductionPolicy");
    app.UseMiddleware<ExceptionHandlingMiddleware>();
}

app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
app.UseMultiTenant();

app.MapControllers();
app.MapFallbackToFile("/index.html");

Log.Information("Application running.");

app.Run();
