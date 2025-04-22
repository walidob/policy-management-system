# Policy Management System - Frontend Guide

## Technology Stack
- Angular 19+
- JWT authentication
- RxJS
- NG Bootstrap

## Architecture
- Standalone components architecture
- Service-based state management
- HTTP interceptors for authentication
- Lazy-loaded feature modules
- Strongly typed interfaces for API models

## Key Features
- Protected routes with guards
- Responsive design
- Optimized API data fetching
- Pagination for policy lists
- Sorting and filtering capabilities

## Form Validation
- Client-side validation using Angular Reactive Forms
- Robust error handling with detailed validation messages
- Required field validation with visual indicators
- Pattern matching for specialized fields (emails, dates, etc.)

## Available Pages
- Login page
- Policies list page 
- Create policy page (Admin only)
- Edit policy page (Admin only)
- View policy page

## Error Handling
- Error messages currently display at the top of components
- Example: `Failed to load policies: Http failure response for https://127.0.0.1:52619/api/policies: 429 Too Many Requests`
- Future: Toast notifications will replace inline errors
- HTTP interceptors capture and process API errors

## Security Features
- JWT token storage in HTTP-only cookies
- Route guards preventing unauthorized access

## Development Commands
- `ng serve` - Start development server
- `ng build` - Build for production
- `ng test` - Run tests 