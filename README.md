# Policy Management System

This project is a Policy Management System built with Angular and .NET Core.

## Technology Stack

- Frontend: Angular (v19+)
- Backend: .NET 9
- Database: SQL Server

## Architecture

### Database Structure
The system uses a multi-tenant database approach:
- Default database: Stores identity users, tenant details and common data
- Tenant databases: Multiple databases with identical schema but separate data

### User Roles
- Super Admin: Has access to all tenants (Except creating/updating policies - not yet implemented)
- Tenant Admin: Can create, edit, delete and view policies from his tenant's database
- Client: Can only read policies

### Application Flow
1. User signs in through the login page
2. System authenticates the user and determines their role
3. Based on role, the user is granted appropriate access permissions to policies
4. All users gets a list of policies based on their role. 

### Authentication and Authorization Flow
- When a user signs in, the system authenticates credentials against the default database
- The system identifies the user's role (Super Admin, Tenant Admin, or Client)
- Role-based UI elements are dynamically shown or hidden based on the user's permissions

## Frontend Structure
The application consists of 5 main pages/components:
- Login page: User authentication
- Policies list page: For everyone
- Create policy page: Form to create new policies (Super Admin and Tenant Admin only)
- Edit policy page: Form to modify existing policies (Super Admin and Tenant Admin only)
- View policy page: For everyone

## Getting Started (Initial Setup)

Make sure you have the prerequisites installed:
- .NET 9 SDK
- Node.js (v20+ recommended)
- Angular CLI (latest v19+)
- SQL Server

### Code Acquisition
Clone the project repository from GitHub:
```
git clone https://github.com/walidob/policy-management-system
cd policy-management-system
```

## Database Setup

### Initial Configuration
1. Update connection strings in `appsettings.json` for both the default and tenant databases:
   Initial Catalog = DefaultDb should stay as is
   Update Data Source, User and Password for DefaultConnection, and Tenants connections, feel free to add as many tenants dbs as you want 

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=[SERVERNAME];Initial Catalog=DefaultDb;Persist Security Info=True;User ID=[USERNAME];Password=[PASSWORD];Trust Server Certificate=True"
  },
  "TenantConfiguration": {
    "Tenants": [
      {
        "Name": "Tenant 1",
        "Identifier": "tenant-1",
        "ConnectionString": "Data Source=[SERVERNAME];Initial Catalog=Tenant-1Db;Persist Security Info=True;User ID=[USERNAME];Password=[PASSWORD];Trust Server Certificate=True",
        "DatabaseIdentifier": "Tenant-1Db"
      }
    ]
  }
}
```

### Database Migrations
The application utilizes Entity Framework Core for database management:

Navigate to your root directory of your repository and execute the below command in powershell 
1. Apply migrations to the default database:
```
dotnet ef database update -c DefaultDbContext -p src/PolicyManagement.Infrastructure -s src/PolicyManagementApp.Api
```

2. Tenant migrations are applied automatically on application startup and their demo data seeded based on feature flags in `appsettings.json`:
```json
{
  "FeatureFlags": {
    "EnableSeedData": true,
    "EnableTenantSeedData": true,
    "EnableTenantMigrations": true
  }
}
```

## Running the Solution

### Default Ports
- Backend API: 
  - HTTPS: https://localhost:57296 and http://localhost:57297
- Frontend Angular application: http://localhost:52619

The Angular application is configured with a proxy to automatically forward API requests to the backend. (DEV ONLY)

### Running the Application
To run the application, update your launchSettings.json with the following configuration, set the API project as the startup project and click Start:

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:52619",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES": "Microsoft.AspNetCore.SpaProxy"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:57296;http://localhost:57297",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES": "Microsoft.AspNetCore.SpaProxy"
      }
    }
  }
}
```

### Manual Startup (Fallback Method)
If the multiple project startup doesn't work as expected, you can run each project separately:

#### Starting the Backend API:
1. In Visual Studio, set the API project as the startup project
   - Right-click on the API project in Solution Explorer
   - Select "Set as Startup Project"
2. Press F5 or click the "Start" button to run the API
3. Note the URL where the API is running:
   - HTTPS: https://localhost:57296

#### Starting the Frontend:
1. Open a command prompt or terminal
2. Navigate to the Angular project directory:
```
cd frontend/policymanagementapp.client
```
3. Install dependencies (if not already done):
```
npm install
```
4. Start the Angular development server:
```
ng serve
```
5. The frontend will be accessible at http://localhost:52619

Make sure the proxy configuration in the Angular project points to the correct API URL. The proxy configuration file (proxy.conf.json) should contain:

```const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:57296';

const PROXY_CONFIG = [
  {
    context: [
      "/api",
      "/scalar", // .NET 9 no longer includes Swagger UI by default; /scalar is used as its replacement for OpenAPI UI.      "/health"
      "/health"
    ],
    target,
    secure: false
  }
]

module.exports = PROXY_CONFIG;

```
## Login Credentials

### Super Admin
- Email: superadmin@tenants.com
- Password: P@ssw0rd

### Tenant Admins
- Email: admin@tenant2.com (for Tenant 2)
- Password: P@ssw0rd

### Tenant Users
- Email: user@tenant1.com (for Tenant 1)
- Password: P@ssw0rd

## Testing

The solution includes comprehensive unit tests covering almost all aspects of the API project.
Tests were generated with the help of AI to ensure maximum coverage despite time constraints.

To run tests:
1. Open Test Explorer in Visual Studio
2. Click "Run All Tests" or select specific test categories to run

## Deployment

### Database Initialization
The application utilizes Entity Framework Core for database management. Before running the application:
1. Configure connection strings in appsettings.json for both default and tenant databases
2. Apply EF Core migration to create the DefaultDB database schema:
```
dotnet ef database update -c DefaultDbContext -p src/PolicyManagement.Infrastructure -s src/PolicyManagementApp.Api
```

### IIS Deployment
The system can be deployed to IIS with the following configurations:
- API: Create an application pool with "No Managed Code" and deploy the published .NET 9 API
- Frontend: Deploy the built Angular application with URL Rewrite Module enabled

#### API Deployment (.NET Core)
1. Choose Deployment Strategy: Framework-Dependent or Self-Contained deployment (depending on the server's environment)
2. Publish the API:
```
create the appsettings.{ENVIRONMENT}.json file set the specific environment's values
right click the Api project and click build 
select the release profile (uat, prod..)
click publish (choose strategy there, Standalone or Framework-dependant)
navigate to your publish folder, compress its content, copy this ZIP file to the server
```
3. Configure IIS Application Pool with "No Managed Code" setting
   - Open IIS Manager
   - Create a new Application Pool (or select existing)
   - Right-click on the Application Pool and select "Advanced Settings"
   - Set "Managed Pipeline Mode" to "Integrated"
   - Set ".NET CLR version" to "No Managed Code"
   - Click OK to save changes
   
4. Create IIS Website
   - Right-click on "Sites" in IIS Manager and select "Add Website"
   - Enter a name for your site (e.g., "PolicyManagementAPI")
   - Set the physical path to your published API folder
     (Typically C:\inetpub\wwwroot\PolicyManagementAPI or your custom folder)
   - Configure binding settings (hostname, port)
   - Click OK to create the website
   
5. Set appropriate permissions on the published folder
   - Right-click the published folder, Properties, Security
   - Add IIS_IUSRS with Read & Execute permissions

#### Frontend Deployment (Angular)
1. Build the frontend:
```
cd frontend/policymanagementapp.client
ng build --configuration {ENVIRONMENT}
```

2. Create IIS Website for frontend
   - Open IIS Manager
   - Create a new website (or application under existing site)
   - Set physical path to the dist/policymanagementapp.client folder
     (Typically C:\inetpub\wwwroot\PolicyManagementUI or your custom folder)
   - Configure binding settings (hostname, port)
   
3. Configure URL Rewrite for SPA routing
   - Create a web.config file in the frontend's root folder with URL rewrite rules for Angular routing
   - Install URL Rewrite Module for IIS if not already installed

4. Update API URL in environment.{ENVIRONMENT}.ts
   - Edit frontend/policymanagementapp.client/src/environments/environment.{ENVIRONMENT}.ts
   - Set apiUrl to your deployed API URL


## API Endpoints
- Health Check: `/health` - Provides basic API status
- API Documentation: `/scalar` - API documentation and testing interface (it is the replacement for Scalar by Microsoft)

## Additional Resources
- For more detailed documentation refer to documents folder in the root directory

## Azure Deployment

The TenantMigrationService class handles creating and migrating tenant databases. When deploying to Azure, modify this class to properly work with Azure SQL databases.

### Azure App Service Deployment
1. Create Azure resources
   - Create Azure App Service for API (.NET 9)
   - Create Azure App Service for Frontend (Static Web App)
   - Create Azure SQL Database for Default and Tenant databases

2. Configure connection strings
   - Store connection strings in Azure Key Vault
   - Update application to read connection strings from Key Vault
   - Make sure your Azure services have appropriate access policies

### CI/CD with GitHub Actions or Azure DevOps

1. Create pipeline for API
   - Connect repository to Azure DevOps or GitHub Actions
   - Configure build pipeline to compile .NET 9 project
   - Set up deployment to Azure App Service
   - Enable automatic deployment on main branch changes

2. Create pipeline for Frontend
   - Configure build pipeline with:
     - npm install
     - npm run build -- --configuration production
     - Deploy to Azure Static Web App
   - Enable automatic deployment on main branch changes

3. Database migrations
   - Add a step in the pipeline to run database migrations:
     ```
     dotnet ef database update -c DefaultDbContext -p src/PolicyManagement.Infrastructure -s src/PolicyManagementApp.Api
     ```
   - Configure tenant database migrations in the startup process
