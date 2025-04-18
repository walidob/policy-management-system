
```bash
dotnet ef migrations add InitialMigration --project src/PolicyManagement.Persistence --startup-project src/PolicyManagementApp.Api -c CatalogDbContext -o Contexts/CatalogDbContext/Migrations
```


```bash
dotnet ef database update --project src/PolicyManagement.Persistence --startup-project src/PolicyManagementApp.Api --context CatalogDbContext
```
