# Policy Management System - Frontend Guide

## What We Built With
- Angular 19.2.0
- NG Bootstrap 18.0.0
- JWT for login security
- RxJS for data handling

## How It's Organized
We use a **standalone components approach** which makes each component more independent and easier to maintain.

## Main Pages
- **Login Page**: Simple form with email/password fields
- **Dashboard**: Overview of all policies with quick actions
- **Policy List**: Searchable table of all policies with sorting
- **Policy Details**: Complete view of a policy with all information
- **Policy Editor**: Form to create or edit policies
- **User Profile**: User information and settings

## Key Features
- **Security**: JWT tokens protect all pages
- **Responsive Design**: Works on all devices
- **Fast Loading**: Pages load quickly with optimized code
- **Error Handling**: Clear messages when something goes wrong

## How Things Work
1. **Login**: Enter credentials → Get JWT token → Access granted
2. **Data Flow**: User action → Service call → API request → Display response
3. **Navigation**: Protected by guards that check if you're logged in

## Main Parts
- **Services**: Connect to the API and handle data
- **Guards**: Check if you can access a page
- **Interceptors**: Add security headers to all requests
- **Components**: What you see on screen

## Development
- Start app: `ng serve`
- Build app: `ng build`
- Test app: `ng test`

## Why We Made These Choices
- **Standalone Components**: Easier to maintain and test
- **NG Bootstrap**: Consistent look and feel without custom CSS
- **Service-based State**: Simple state management without extra libraries
- **JWT**: Industry standard for secure authentication 