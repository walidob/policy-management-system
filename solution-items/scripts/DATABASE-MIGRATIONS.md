Default Database: Create and apply migration:

dotnet ef migrations add InitialMigration -c DefaultDbContext -o Migrations/DefaultDb -p src/PolicyManagement.Infrastructure -s src/PolicyManagementApp.Api
dotnet ef database update -c DefaultDbContext -p src/PolicyManagement.Infrastructure -s src/PolicyManagementApp.Api


Tenant Database Create only, no apply (it is done dynamically on runtime)

dotnet ef migrations add TenantDbInitialMigration -c TenantDbContextBase -o Migrations/Tenant -p src/PolicyManagement.Infrastructure -s src/PolicyManagementApp.Api
