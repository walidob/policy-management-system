# Policy Management System

This project is a Policy Management System built with Angular and .NET Core.

## Current Status

- Basic project structure is set up.
- Uses Angular for the frontend and .NET Core for the backend API.
- Folders include `frontend/`, `src/`, and `tests/`.

## Technology

- Frontend: Angular (v19+)
- Backend: .NET 9
- Database: SQL Server

## Architecture

### Database Structure
The system uses a multi-tenant database approach:
- Default database: Stores identity users, tenant details, and system-wide configuration
- Tenant databases: Multiple databases with identical schema but separate data for each tenant

### User Roles
- Super Admin: Has access to all tenants' policies and CRUD operations
- Tenant Admin: Can only access, edit, and read their tenant's details
- Client: Can only read policies (no edit/create/delete permissions)

### Application Flow
1. User signs in through the login page
2. System authenticates the user and determines their role
3. Based on role, the user is granted appropriate access permissions to policies
4. Navigation is provided to the appropriate pages based on user role

## Frontend Structure
The application consists of four main pages:
- Login page: User authentication
- Policies list page: View all accessible policies
- Create policy page: Form to create new policies (Super Admin and Tenant Admin only)
- Edit policy page: Form to modify existing policies (Super Admin and Tenant Admin only)

## Getting Started (Initial Setup)

Make sure you have the prerequisites installed:
- .NET 9 SDK
- Node.js (v20+ recommended)
- Angular CLI (latest v19+)
- SQL Server

## Running the Solution

### Running Multiple Projects
1. Open the solution in Visual Studio
2. Right-click on the solution in Solution Explorer
3. Select "Configure Startup Projects..."
4. Choose "Multiple startup projects" option
5. Set both the API and frontend projects to "Start"
6. Click "Apply" and "OK"
7. Press F5 or click the "Start" button to run both projects simultaneously

### Performance Tip
When working solely on the API, you can unload the frontend project to achieve faster build times:
1. Right-click on the frontend project in Solution Explorer
2. Select "Unload Project"
3. When you need to work with the frontend again, right-click the unloaded project and select "Reload Project"

## Deployment

### Database Initialization
The application utilizes Entity Framework Core for database management. Before running the application:
1. Configure connection strings in appsettings.json for both default and tenant databases
2. Apply EF Core migrations to create or update the database schema:
```
dotnet ef database update -c DefaultDbContext -p src/PolicyManagement.Infrastructure -s src/PolicyManagementApp.Api
```

### IIS Deployment
The system can be deployed to IIS with the following configurations:
- API: Create an application pool with "No Managed Code" and deploy the published .NET 9 API
- Frontend: Deploy the built Angular application with URL Rewrite Module enabled

See the detailed deployment guide in the solution documentation for step-by-step instructions.

## API Endpoints
- Health Check: `/health` - Provides basic API status
- API Documentation: `/scalar` - Interactive API documentation and testing interface

## Additional Resources
- For more detailed documentation on specific components, refer to additional documentation in the solution-items folder
