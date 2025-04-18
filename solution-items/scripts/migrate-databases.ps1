# Database Migration Script
# This script creates and applies Entity Framework Core migrations for multiple database contexts

$ErrorActionPreference = 'Stop'

# Navigate to the root directory of the solution to ensure paths work correctly
Push-Location (Join-Path $PSScriptRoot "..\..") 
$rootPath = Get-Location
Write-Host "Working from solution root: $rootPath" -ForegroundColor Cyan

$projectPath = "src/PolicyManagement.Persistence"
$startupProject = "src/PolicyManagementApp.Api"

Write-Host "Starting database migration process..." -ForegroundColor Cyan

try {
    # Create migrations for each context
    Write-Host "Creating migrations..." -ForegroundColor Yellow
    
    Write-Host "Creating migration for CatalogDbContext..." -ForegroundColor Green
    dotnet ef migrations add InitialMigration --project $projectPath --startup-project $startupProject -c CatalogDbContext -o Contexts/CatalogDbContext/Migrations
    
    Write-Host "Creating migration for Tenant1DbContext..." -ForegroundColor Green
    dotnet ef migrations add InitialMigration --project $projectPath --startup-project $startupProject -c Tenant1DbContext -o Contexts/TenantsDbContexts/Migrations/Tenant1
    
    Write-Host "Creating migration for Tenant2DbContext..." -ForegroundColor Green
    dotnet ef migrations add InitialMigration --project $projectPath --startup-project $startupProject -c Tenant2DbContext -o Contexts/TenantsDbContexts/Migrations/Tenant2
    
    # Apply migrations to the database
    Write-Host "Applying migrations to databases..." -ForegroundColor Yellow
    
    Write-Host "Updating CatalogDbContext database..." -ForegroundColor Green
    dotnet ef database update --project $projectPath --startup-project $startupProject --context CatalogDbContext
    
    Write-Host "Updating Tenant1DbContext database..." -ForegroundColor Green
    dotnet ef database update --project $projectPath --startup-project $startupProject -c Tenant1DbContext
    
    Write-Host "Updating Tenant2DbContext database..." -ForegroundColor Green
    dotnet ef database update --project $projectPath --startup-project $startupProject -c Tenant2DbContext
    
    Write-Host "Database migration completed successfully!" -ForegroundColor Cyan
}
catch {
    Write-Host "An error occurred during the migration process:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host "Make sure Entity Framework Core tools are installed. If not, run: dotnet tool install --global dotnet-ef" -ForegroundColor Yellow
    exit 1
}
finally {
    # Return to the original directory
    Pop-Location
}
