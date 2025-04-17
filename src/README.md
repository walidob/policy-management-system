# Backend API (.NET Core)

This folder contains the source code for the .NET Core backend API.

## Structure

The backend follows Clean Architecture principles, organized into:
- `PolicyManagementApp.Api` (API Layer)
- `PolicyManagement.Application` (Application Layer)
- `PolicyManagement.Domain` (Domain Layer)
- `PolicyManagement.Infrastructure` (Infrastructure Layer)
- `PolicyManagement.Persistence` (Persistence Layer)

## Development Guidelines

1. Follow Clean Architecture principles

- **Logging:** Integrated Serilog including configured file output.
- **API Error Handling:** Exception handling middleware in the API project.
