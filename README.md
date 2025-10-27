# JupiterDMS - Enterprise Document Management System

A comprehensive, enterprise-level .NET 8 Document Management System built with Clean Architecture, Domain-Driven Design (DDD), CQRS, and Repository + Unit of Work patterns.

## 🏗️ Architecture Overview

JupiterDMS follows a layered architecture with clear separation of concerns:

```
JupiterDMS.sln
├── JupiterDMS.Domain                     ← Domain layer (Entities, Value Objects, Enums)
├── JupiterDMS.Application                ← Application layer (CQRS, DTOs, Interfaces)
├── JupiterDMS.Infrastructure.DataAccess  ← Data Access layer (EF Core, DbContext, Repositories)
├── JupiterDMS.Infrastructure.Infra       ← Infrastructure layer (File Storage, Logging, Services)
├── JupiterDMS.API                        ← ASP.NET Core Web API (Controllers, Swagger, DI Root)
└── JupiterDMS.WebUI                      ← ASP.NET Core MVC (Razor UI, HttpClient to API)
```

## 🎯 Key Features

### Domain Layer
- **Entities**: Library, Folder, Document, DocumentVersion, User, AuditLog
- **Value Objects**: Domain events and exceptions
- **Enums**: UserRole, AuditActionType, DocumentStatus
- **Constants**: Domain-wide constants with no magic strings
- **Domain Events**: DocumentUploadedEvent, LibraryCreatedEvent

### Application Layer
- **CQRS Pattern**: Separate commands and queries using MediatR
- **Validators**: FluentValidation for all commands
- **DTOs**: Data transfer objects for API communication
- **AutoMapper**: Automatic entity-to-DTO mapping
- **Pipeline Behaviors**: Validation behavior for all requests

### Infrastructure.DataAccess Layer
- **EF Core 8**: Code-First approach with SQL Server LocalDB
- **DbContext**: JupiterDbContext with all entity configurations
- **Entity Configurations**: Fluent API configurations for all entities
- **Repositories**: Generic repository with async operations
- **Unit of Work**: Transaction management and repository coordination

### Infrastructure.Infra Layer
- **File Storage Service**: Local file system storage with hash calculation
- **Serilog Logging**: Structured logging to console and files
- **HttpClient Factory**: Typed HTTP clients for external services

### API Layer
- **RESTful Controllers**: Thin controllers that delegate to MediatR
- **Swagger/OpenAPI**: Full API documentation with XML comments
- **Exception Handling**: Global middleware for consistent error responses
- **Health Checks**: Built-in health check endpoint

### WebUI Layer
- **MVC Pattern**: Razor views with Bootstrap styling
- **Typed HttpClient**: JupiterDmsApiClient for API communication
- **View Models**: Separate models for UI concerns
- **Responsive Design**: Mobile-friendly Bootstrap layout

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server LocalDB (Windows Authentication)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd JupiterDMS
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Create the database**
   ```bash
   cd JupiterDMS.Infrastructure.DataAccess
   dotnet ef database update
   ```

4. **Run the API**
   ```bash
   cd JupiterDMS.API
   dotnet run
   ```

5. **Run the WebUI** (in another terminal)
   ```bash
   cd JupiterDMS.WebUI
   dotnet run
   ```

### Access Points
- **API**: https://localhost:7001
- **Swagger UI**: https://localhost:7001/swagger
- **WebUI**: https://localhost:7002
- **Health Check**: https://localhost:7001/health

## 📋 Database Configuration

The application uses SQL Server LocalDB with Windows Authentication (no username/password required).

**Connection String** (appsettings.json):
```json
"ConnectionStrings": {
  "JupiterDmsConnection": "Server=(localdb)\\mssqllocaldb;Database=JupiterDMS;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

## 🔧 Configuration

### File Storage
Configure the file storage path in `appsettings.json`:
```json
"FileStorage": {
  "Path": "storage"
}
```

### API Settings (WebUI)
Configure the API base URL in `appsettings.json`:
```json
"ApiSettings": {
  "BaseUrl": "https://localhost:7001"
}
```

## 📚 API Endpoints

### Libraries
- `GET /api/libraries` - Get all libraries
- `GET /api/libraries/{id}` - Get library by ID
- `POST /api/libraries` - Create new library
- `PUT /api/libraries/{id}` - Update library
- `DELETE /api/libraries/{id}` - Delete library

## 🏛️ Design Patterns

### CQRS (Command Query Responsibility Segregation)
- Commands modify state (Create, Update, Delete)
- Queries retrieve data (GetAll, GetById)
- Separate handlers for each operation

### Repository Pattern
- Generic `IRepository<T>` interface
- Async operations throughout
- LINQ expression support for filtering

### Unit of Work Pattern
- Coordinates multiple repositories
- Manages database transactions
- Single SaveChangesAsync call

### Dependency Injection
- Extension methods for each layer (AddApplication, AddDataAccess, AddInfra)
- Scoped lifetime for DbContext and repositories
- Transient lifetime for handlers and validators

## 📝 Best Practices Implemented

✅ **No Magic Strings**: All constants defined in `DomainConstants`
✅ **Async/Await**: All I/O operations are asynchronous
✅ **XML Documentation**: Full documentation on public APIs
✅ **Validation**: FluentValidation on all commands
✅ **Error Handling**: Global exception middleware
✅ **Logging**: Structured logging with Serilog
✅ **Soft Deletes**: IsDeleted flag on all entities
✅ **Audit Trail**: AuditLog entity for tracking changes
✅ **Entity Relationships**: Proper foreign keys and navigation properties
✅ **Indexes**: Strategic indexes for performance

## 🧪 Testing Recommendations

1. **Unit Tests**: Test domain logic and validators
2. **Integration Tests**: Test repositories and handlers
3. **API Tests**: Test controllers and endpoints
4. **UI Tests**: Test MVC controllers and views

## 📦 NuGet Dependencies

### Domain
- None (pure domain logic)

### Application
- MediatR 12.4.1
- FluentValidation 11.10.0
- AutoMapper 13.0.1
- Microsoft.Extensions.DependencyInjection.Abstractions 8.0.0

### Infrastructure.DataAccess
- Microsoft.EntityFrameworkCore 8.0.0
- Microsoft.EntityFrameworkCore.SqlServer 8.0.0
- Microsoft.EntityFrameworkCore.Design 8.0.0
- Microsoft.EntityFrameworkCore.Tools 8.0.0

### Infrastructure.Infra
- Serilog 4.1.0
- Serilog.AspNetCore 8.0.3
- Serilog.Sinks.Console 6.0.0
- Serilog.Sinks.File 6.0.0

### API
- Swashbuckle.AspNetCore 6.8.1
- Microsoft.AspNetCore.Mvc.NewtonsoftJson 8.0.0
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0

### WebUI
- Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation 8.0.0
- System.Net.Http.Json 8.0.0

## 🔐 Security Considerations

- Windows Authentication for database (no credentials in code)
- HTTPS redirection in production
- HSTS headers enabled
- CORS policy configured
- Input validation on all commands
- SQL injection prevention through EF Core

## 📈 Future Enhancements

- [ ] JWT Authentication
- [ ] Role-based authorization
- [ ] Document versioning UI
- [ ] Full-text search
- [ ] Document preview
- [ ] Workflow engine
- [ ] Audit log viewer
- [ ] Bulk operations
- [ ] API rate limiting
- [ ] Caching layer

## 📄 License

This project is provided as-is for educational and enterprise use.

## 👥 Contributing

Contributions are welcome! Please follow the established patterns and maintain code quality.

## 📞 Support

For issues and questions, please refer to the documentation or create an issue in the repository.

---

**Built with ❤️ using .NET 8, Clean Architecture, and DDD principles**

