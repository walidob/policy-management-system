# CatalogDbContext Migrations

This directory contains the migrations for the CatalogDbContext database. 

## Connection String

The connection string in appsettings.json should use the key "CatalogConnection"

## Migration Commands

To create a new migration:

```bash
dotnet ef migrations add InitialMigration --project src/PolicyManagement.Persistence --startup-project src/PolicyManagementApp.Api -c CatalogDbContext -o Contexts/CatalogDbContext/Migrations
```

To apply migrations to the database:

```bash
dotnet ef database update --project src/PolicyManagement.Persistence --startup-project src/PolicyManagementApp.Api --context CatalogDbContext
```
