# JupiterDMS Demo Users

This document contains the credentials for the three demo users that are automatically created when the JupiterDMS API starts.

## Demo User Accounts

### 1. Admin User
- **Username**: `admin`
- **Password**: `Admin123!`
- **Email**: `admin@jupiterdms.com`
- **Role**: Administrator
- **Permissions**: Full access to all features

### 2. Editor User
- **Username**: `editor`
- **Password**: `Editor123!`
- **Email**: `editor@jupiterdms.com`
- **Role**: Editor
- **Permissions**: Can view and edit documents

### 3. Viewer User
- **Username**: `viewer`
- **Password**: `Viewer123!`
- **Email**: `viewer@jupiterdms.com`
- **Role**: Viewer
- **Permissions**: Read-only access to documents

## Automatic Setup

When you run the JupiterDMS API:

1. **Database migrations are applied automatically** - No need to run `update-database` command
2. **Demo users are created automatically** - If they don't already exist
3. **Default library and folders are created** - Ready for immediate use

## API Endpoints

The API runs on:
- **HTTP**: http://localhost:5001 (recommended for Office addins)
- **HTTPS**: https://localhost:7001

## Office Addins Integration

The Office addins are pre-configured to connect to the API server automatically. Users only need to enter their username and password - no server URL required.

## Testing

You can test the login functionality using any HTTP client:

```bash
POST http://localhost:5001/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin123!"
}
```

All three users should return a successful login response with a JWT token.
