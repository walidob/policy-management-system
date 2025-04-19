# Policy Management System

This project is a Policy Management System built with Angular and .NET Core.

## Current Status

- Basic project structure is set up.
- Uses Angular for the frontend and .NET Core for the backend API.
- Folders include `frontend/`, `src/`, and `tests/`.

## Technology

- Frontend: Angular
- Backend: .NET Core
- Database: SQL Server

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
