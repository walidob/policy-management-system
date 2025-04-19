# Database Migration Script
# This script creates and applies Entity Framework Core migrations for multiple database contexts

$ErrorActionPreference = 'Stop'

# Navigate to the root directory of the solution to ensure paths work correctly
Push-Location (Join-Path $PSScriptRoot "..\..") 
$rootPath = Get-Location
Write-Host "Working from solution root: $rootPath" -ForegroundColor Cyan

$projectPath = "src/PolicyManagement.Infrastructure"
$startupProject = "src/PolicyManagementApp.Api"

Write-Host "Starting database migration process..." -ForegroundColor Cyan

try {
    # Create migrations for each context
    Write-Host "Creating migrations..." -ForegroundColor Yellow
    
    Write-Host "Creating migration for DefaultDbContext..." -ForegroundColor Green
    dotnet ef migrations add InitialMigration1 -c DefaultDbContext -o Migrations/DefaultDb -p $projectPath -s $startupProject
    
    Write-Host "Creating migration for TenantDbContextBase..." -ForegroundColor Green
    dotnet ef migrations add TenantDbInitialMigration1 -c TenantDbContextBase -o Migrations/Tenant -p $projectPath -s $startupProject
    
    # Apply migrations to the database
    Write-Host "Applying migrations to databases..." -ForegroundColor Yellow
    
    Write-Host "Updating DefaultDbContext database..." -ForegroundColor Green
    dotnet ef database update -c DefaultDbContext -p $projectPath -s $startupProject
    
    Write-Host "Migration process completed successfully!" -ForegroundColor Cyan
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
