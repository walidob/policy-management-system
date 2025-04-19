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
    .AddResponseCaching(); ;

builder.Services.AddControllers();
builder.Services.AddMemoryCache(); 
builder.Services.AddOpenApi();


var app = builder.Build();
Log.Information("Application built.");


await app.Services.SeedDefaultDbDataAsync();
await app.Services.ApplyTenantsDbsMigrationsAsync();
await app.Services.SeedTenantsDbsDataAsync();

Log.Information("Configuring middleware pipeline.");

app.UseDefaultFiles();
app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseSerilogRequestLogging();
}
else
{
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseHsts();
}

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();
app.UseMultiTenant();

app.MapControllers();

app.MapFallbackToFile("/index.html");

Log.Information("Application running.");

app.Run();
