# JupiterDMS Setup Guide

## Prerequisites

- **.NET 8 SDK** - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
- **SQL Server LocalDB** - Installed with Visual Studio or SQL Server Express
- **Visual Studio 2022** or **VS Code** with C# extension
- **Git** - For version control

## Step-by-Step Setup

### 1. Clone the Repository

```bash
git clone <repository-url>
cd JupiterDMS
```

### 2. Verify .NET Installation

```bash
dotnet --version
```

Should output version 8.0.x or higher.

### 3. Restore NuGet Packages

```bash
dotnet restore
```

This downloads all required NuGet packages for all projects.

### 4. Create the Database

Navigate to the Infrastructure.DataAccess project and create the initial migration:

```bash
cd JupiterDMS.Infrastructure.DataAccess
dotnet ef migrations add InitialCreate
dotnet ef database update
```

This creates the JupiterDMS database in LocalDB with all tables.

**Verify the database was created:**
- Open SQL Server Object Explorer in Visual Studio
- Connect to `(localdb)\mssqllocaldb`
- You should see the `JupiterDMS` database

### 5. Build the Solution

```bash
cd ..
dotnet build
```

Verify all projects build successfully without errors.

### 6. Run the API

```bash
cd JupiterDMS.API
dotnet run
```

The API will start on:
- **HTTPS**: https://localhost:7001
- **HTTP**: http://localhost:5001

You should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
```

### 7. Access Swagger UI

Open your browser and navigate to:
```
https://localhost:7001/swagger
```

You should see the Swagger UI with all API endpoints documented.

### 8. Run the WebUI (in a new terminal)

```bash
cd JupiterDMS.WebUI
dotnet run
```

The WebUI will start on:
- **HTTPS**: https://localhost:7002
- **HTTP**: http://localhost:5002

### 9. Access the Application

Open your browser and navigate to:
```
https://localhost:7002
```

You should see the JupiterDMS home page with navigation to Libraries.

## Troubleshooting

### Issue: "Connection string not found"

**Solution**: Verify `appsettings.json` in the API project contains:
```json
"ConnectionStrings": {
  "JupiterDmsConnection": "Server=(localdb)\\mssqllocaldb;Database=JupiterDMS;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

### Issue: "Database does not exist"

**Solution**: Run the migrations:
```bash
cd JupiterDMS.Infrastructure.DataAccess
dotnet ef database update
```

### Issue: "Port already in use"

**Solution**: Change the port in `launchSettings.json` or kill the process using the port:
```bash
# Windows
netstat -ano | findstr :7001
taskkill /PID <PID> /F

# Linux/Mac
lsof -i :7001
kill -9 <PID>
```

### Issue: "SSL certificate error"

**Solution**: Trust the development certificate:
```bash
dotnet dev-certs https --trust
```

### Issue: "Cannot connect to API from WebUI"

**Solution**: Verify the API base URL in `JupiterDMS.WebUI/appsettings.json`:
```json
"ApiSettings": {
  "BaseUrl": "https://localhost:7001"
}
```

## Project Structure

```
JupiterDMS/
├── JupiterDMS.Domain/                    # Domain entities and logic
│   ├── Common/                           # Base classes
│   ├── Constants/                        # Domain constants
│   ├── Entities/                         # Domain entities
│   ├── Enums/                            # Enumerations
│   ├── Events/                           # Domain events
│   └── Exceptions/                       # Domain exceptions
│
├── JupiterDMS.Application/               # Application services
│   ├── Common/                           # Interfaces and behaviors
│   ├── Features/                         # Feature modules
│   │   └── Libraries/                    # Library feature
│   │       ├── Commands/                 # CQRS commands
│   │       ├── Queries/                  # CQRS queries
│   │       ├── Handlers/                 # Command/Query handlers
│   │       ├── Validators/               # FluentValidation validators
│   │       ├── Dtos/                     # Data transfer objects
│   │       └── Mapping/                  # AutoMapper profiles
│   └── DependencyInjection.cs            # DI configuration
│
├── JupiterDMS.Infrastructure.DataAccess/ # Data access layer
│   ├── Persistence/                      # DbContext
│   ├── Configurations/                   # Entity configurations
│   ├── Repositories/                     # Repository implementations
│   └── DependencyInjection.cs            # DI configuration
│
├── JupiterDMS.Infrastructure.Infra/      # Infrastructure services
│   ├── Services/                         # Business services
│   ├── Logging/                          # Serilog configuration
│   └── DependencyInjection.cs            # DI configuration
│
├── JupiterDMS.API/                       # Web API
│   ├── Controllers/                      # API controllers
│   ├── Middleware/                       # Custom middleware
│   ├── Program.cs                        # Application startup
│   └── appsettings.json                  # Configuration
│
└── JupiterDMS.WebUI/                     # MVC Web UI
    ├── Controllers/                      # MVC controllers
    ├── Services/                         # API client services
    ├── Views/                            # Razor views
    ├── Program.cs                        # Application startup
    └── appsettings.json                  # Configuration
```

## Database Migrations

### Create a New Migration

```bash
cd JupiterDMS.Infrastructure.DataAccess
dotnet ef migrations add <MigrationName>
```

### Apply Migrations

```bash
dotnet ef database update
```

### Revert Last Migration

```bash
dotnet ef migrations remove
```

### View Migration History

```bash
dotnet ef migrations list
```

## Development Workflow

1. **Make changes** to domain entities or add new features
2. **Create a migration** if database schema changes
3. **Update the database** with the new migration
4. **Run tests** to verify changes
5. **Commit and push** to version control

## Configuration Files

### API Configuration (appsettings.json)
- Database connection string
- File storage path
- Logging levels

### WebUI Configuration (appsettings.json)
- API base URL
- Logging levels

## Logging

Logs are written to:
- **Console**: Real-time output
- **Files**: `logs/jupiterdms-YYYY-MM-DD.txt` (API)
- **Files**: `logs/webui-YYYY-MM-DD.txt` (WebUI)

## Next Steps

1. ✅ Explore the Swagger UI to understand the API
2. ✅ Create a library through the WebUI
3. ✅ Review the code structure and patterns
4. ✅ Add new features following the established patterns
5. ✅ Write tests for new functionality

## Additional Resources

- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation](https://fluentvalidation.net/)
- [AutoMapper](https://automapper.org/)
- [Serilog](https://serilog.net/)

## Support

For issues or questions, refer to the README.md or create an issue in the repository.

