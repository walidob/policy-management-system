# Policy Management System

A multi-tenant policy management system built with Angular 19+ and .NET 9.

## Technology Stack

- **Frontend**: Angular 19+
- **Backend**: .NET 9
- **Database**: SQL Server

## Architecture

- **Multi-tenant database approach**:
  - Default database: Identity users, tenant details, common data
  - Tenant databases: Separate data isolation per tenant

- **Clean Architecture** with layers:
  - API Layer: Controllers, routing, caching
  - Application Layer: Business logic, DTOs, validation
  - Domain Layer: Entities, domain rules
  - Infrastructure Layer: Services, cross-cutting concerns

## Key Features

- **Multi-Tenant Architecture** with Finbuckle.MultiTenant
  - Tenant-specific databases with claim-based resolution
  - Automatic migrations across tenant databases

- **RESTful API** with proper HTTP methods and status codes
  - Response caching with tiered expiration (5-15 minutes) (Currently set for MetadataController)
  - OpenAPI documentation with Scalar
  - HATEOAS (To be implemented in future versions)
  - API Versioning (To be implemented in future versions)

> Note: API Versioning and HATEOAS will be implemented in future versions to fully comply with RESTful API principles.

- **Performance Optimization**
  - Response caching and compression
  - Rate limiting (50 requests/minute per IP)

- **Security**
  - JWT authentication with secure configuration

- **Data Access**
  - Repository pattern with Unit of Work
  - Entity Framework Core with automatic migrations

## Users & Roles

- **Super Admin**: Access to all tenants
- **Tenant Admin**: Manage policies for specific tenant
- **Client**: Read-only access to policies

## Login Credentials

- **Super Admin**: superadmin@tenants.com / P@ssw0rd
- **Tenant Admin**: admin@tenant2.com / P@ssw0rd
- **Client**: user@tenant1.com / P@ssw0rd

## API Endpoints
- Health Check: `/health`
- API Documentation: `/scalar`

## Rate Limiting 
When exceeding the rate limit (50 requests/minute per IP), the API will return:
- Status Code: 429 Too Many Requests
- Response Headers: Retry-After: 60
- Response Body:
```json
{
  "title": "Too Many Requests",
  "status": 429,
  "detail": "You've exceeded the rate limit. Please try again later."
}
```
Example error: `Http failure response for https://127.0.0.1:52619/api/policies?pageNumber=1&pageSize=10&sortColumn=id&sortDirection=asc: 429 Too Many Requests`
