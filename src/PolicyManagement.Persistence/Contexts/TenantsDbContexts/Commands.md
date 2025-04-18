
```bash
dotnet ef migrations add InitialMigration --project src/PolicyManagement.Persistence --startup-project src/PolicyManagementApp.Api -c Tenant1DbContext -o Contexts/TenantsDbContexts/Migrations/Tenant1
dotnet ef migrations add InitialMigration --project src/PolicyManagement.Persistence --startup-project src/PolicyManagementApp.Api -c Tenant2DbContext -o Contexts/TenantsDbContexts/Migrations/Tenant2

```bash
dotnet ef database update --project src/PolicyManagement.Persistence --startup-project src/PolicyManagementApp.Api -c Tenant1DbContext
dotnet ef database update --project src/PolicyManagement.Persistence --startup-project src/PolicyManagementApp.Api -c Tenant2DbContext

```
