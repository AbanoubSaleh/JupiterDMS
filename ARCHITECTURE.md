# JupiterDMS Architecture Documentation

## Overview

JupiterDMS is built using **Clean Architecture** with **Domain-Driven Design (DDD)**, **CQRS (Command Query Responsibility Segregation)**, and the **Repository + Unit of Work** patterns. This document explains the architectural decisions and how each layer works.

## Architectural Layers

### 1. Domain Layer (JupiterDMS.Domain)

**Purpose**: Contains pure business logic with no external dependencies.

**Responsibilities**:
- Define domain entities and value objects
- Implement domain logic and business rules
- Define domain events
- Define domain exceptions
- Provide constants and enumerations

**Key Components**:

#### Entities
- `Library`: Top-level container for documents
- `Folder`: Hierarchical folder structure
- `Document`: Document with versioning support
- `DocumentVersion`: Version history of documents
- `User`: System users with roles
- `AuditLog`: Audit trail for compliance

#### Base Classes
- `BaseEntity`: Common properties (Id, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, IsDeleted)

#### Enums
- `UserRole`: Guest, User, PowerUser, Administrator, SystemAdministrator
- `AuditActionType`: Create, Update, Delete, View, Download, Upload, Login, Logout, etc.
- `DocumentStatus`: Draft, PendingReview, Approved, Published, Archived, Obsolete

#### Constants
- `DomainConstants`: No magic strings - all constants defined here

#### Domain Events
- `IDomainEvent`: Marker interface for domain events
- `DocumentUploadedEvent`: Raised when document is uploaded
- `LibraryCreatedEvent`: Raised when library is created

#### Exceptions
- `DomainException`: Base exception for domain errors
- `EntityNotFoundException`: Thrown when entity not found

**Design Principles**:
- ✅ No dependencies on other projects
- ✅ No database access
- ✅ No external service calls
- ✅ Pure business logic only

### 2. Application Layer (JupiterDMS.Application)

**Purpose**: Implements use cases and orchestrates domain logic.

**Responsibilities**:
- Implement CQRS commands and queries
- Validate input using FluentValidation
- Map entities to DTOs using AutoMapper
- Coordinate with repositories
- Implement cross-cutting concerns (validation, logging)

**Key Components**:

#### CQRS Pattern
```
Command (modifies state)
  ↓
Validator (FluentValidation)
  ↓
Handler (executes business logic)
  ↓
Repository (persists changes)
  ↓
SaveChanges (Unit of Work)

Query (reads state)
  ↓
Handler (retrieves data)
  ↓
Repository (queries data)
  ↓
Mapper (converts to DTO)
  ↓
Return DTO
```

#### Feature Organization
```
Features/
  Libraries/
    Commands/
      CreateLibraryCommand.cs
      UpdateLibraryCommand.cs
      DeleteLibraryCommand.cs
    Queries/
      GetAllLibrariesQuery.cs
      GetLibraryByIdQuery.cs
    Handlers/
      CreateLibraryCommandHandler.cs
      UpdateLibraryCommandHandler.cs
      DeleteLibraryCommandHandler.cs
      GetAllLibrariesQueryHandler.cs
      GetLibraryByIdQueryHandler.cs
    Validators/
      CreateLibraryCommandValidator.cs
      UpdateLibraryCommandValidator.cs
    Dtos/
      LibraryDto.cs
    Mapping/
      LibraryMappingProfile.cs
```

#### Interfaces
- `IRepository<T>`: Generic repository interface
- `IUnitOfWork`: Transaction management
- `IFileStorageService`: File storage operations

#### Behaviors
- `ValidationBehavior<TRequest, TResponse>`: Automatic validation pipeline

**Design Principles**:
- ✅ Depends on Domain layer only
- ✅ No direct database access (uses repositories)
- ✅ All I/O operations are async
- ✅ Validation before execution

### 3. Infrastructure.DataAccess Layer

**Purpose**: Handles all database operations using Entity Framework Core.

**Responsibilities**:
- Configure DbContext
- Define entity configurations
- Implement repositories
- Implement Unit of Work
- Manage migrations

**Key Components**:

#### DbContext
```csharp
public class JupiterDbContext : DbContext
{
    public DbSet<Library> Libraries { get; set; }
    public DbSet<Folder> Folders { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentVersion> DocumentVersions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
}
```

#### Entity Configurations
- Fluent API configuration for each entity
- Indexes for performance
- Foreign key relationships
- Cascade delete policies
- Column constraints and defaults

#### Generic Repository
```csharp
public class GenericRepository<TEntity> : IRepository<TEntity>
{
    // Async CRUD operations
    // LINQ expression support
    // Filtering and counting
}
```

#### Unit of Work
```csharp
public class UnitOfWork : IUnitOfWork
{
    public IRepository<Library> Libraries { get; }
    public IRepository<Folder> Folders { get; }
    // ... other repositories
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    public async Task BeginTransactionAsync(CancellationToken cancellationToken);
    public async Task CommitTransactionAsync(CancellationToken cancellationToken);
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken);
}
```

**Database Configuration**:
- SQL Server LocalDB with Windows Authentication
- Connection string in appsettings.json
- Retry policy for transient failures
- Code-First migrations

**Design Principles**:
- ✅ Depends on Domain and Application layers
- ✅ All operations are async
- ✅ Lazy initialization of repositories
- ✅ Transaction support

### 4. Infrastructure.Infra Layer

**Purpose**: Provides cross-cutting infrastructure services.

**Responsibilities**:
- File storage operations
- Logging configuration
- External service integration
- Utility services

**Key Components**:

#### File Storage Service
```csharp
public class FileStorageService : IFileStorageService
{
    public async Task<string> SaveFileAsync(string fileName, Stream fileStream);
    public async Task<Stream> GetFileAsync(string filePath);
    public async Task DeleteFileAsync(string filePath);
    public async Task<bool> FileExistsAsync(string filePath);
    public async Task<string> CalculateFileHashAsync(Stream fileStream);
}
```

#### Serilog Configuration
- Console sink for development
- File sink with daily rolling
- Structured logging with enrichment
- 30-day retention policy

**Design Principles**:
- ✅ Depends on Application layer
- ✅ Provides implementations of Application interfaces
- ✅ Configurable through dependency injection

### 5. API Layer (JupiterDMS.API)

**Purpose**: Exposes REST endpoints for client applications.

**Responsibilities**:
- Define API controllers
- Handle HTTP requests/responses
- Implement global exception handling
- Configure Swagger/OpenAPI
- Setup dependency injection

**Key Components**:

#### Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
public class LibrariesController : ControllerBase
{
    // Thin controllers - delegate to MediatR
    // All operations are async
    // Proper HTTP status codes
}
```

#### Exception Handling Middleware
```csharp
public class ExceptionHandlingMiddleware
{
    // Catches all exceptions
    // Returns consistent error responses
    // Logs errors
    // Handles validation errors specially
}
```

#### Swagger Configuration
- XML documentation from code comments
- Organized by tags
- Request/response examples
- Error response documentation

**Design Principles**:
- ✅ Thin controllers (delegate to MediatR)
- ✅ Consistent error responses
- ✅ Full API documentation
- ✅ Proper HTTP semantics

### 6. WebUI Layer (JupiterDMS.WebUI)

**Purpose**: Provides user interface for the application.

**Responsibilities**:
- Render Razor views
- Handle user interactions
- Communicate with API
- Display data and forms

**Key Components**:

#### Typed HttpClient
```csharp
public class JupiterDmsApiClient
{
    // Typed client for API communication
    // Handles serialization/deserialization
    // Error handling
}
```

#### MVC Controllers
```csharp
public class LibrariesController : Controller
{
    // Delegate to API client
    // Handle view models
    // Render views
}
```

#### Views
- Bootstrap responsive design
- Form validation
- Data display tables
- Navigation

**Design Principles**:
- ✅ Separation of concerns
- ✅ Typed HTTP client
- ✅ View models for UI concerns
- ✅ Responsive design

## Data Flow

### Command Execution Flow
```
1. HTTP POST /api/libraries
   ↓
2. LibrariesController.Create(CreateLibraryCommand)
   ↓
3. MediatR.Send(command)
   ↓
4. ValidationBehavior validates command
   ↓
5. CreateLibraryCommandHandler.Handle()
   ↓
6. Create Library entity
   ↓
7. unitOfWork.Libraries.AddAsync(library)
   ↓
8. unitOfWork.SaveChangesAsync()
   ↓
9. DbContext.SaveChangesAsync()
   ↓
10. SQL Server persists data
    ↓
11. AutoMapper maps Library → LibraryDto
    ↓
12. Return 201 Created with LibraryDto
```

### Query Execution Flow
```
1. HTTP GET /api/libraries
   ↓
2. LibrariesController.GetAll(GetAllLibrariesQuery)
   ↓
3. MediatR.Send(query)
   ↓
4. GetAllLibrariesQueryHandler.Handle()
   ↓
5. unitOfWork.Libraries.FindAsync(predicate)
   ↓
6. DbContext queries SQL Server
   ↓
7. AutoMapper maps Library[] → LibraryDto[]
   ↓
8. Return 200 OK with LibraryDto[]
```

## Dependency Injection

### Registration Order
```csharp
// Program.cs
builder.Services.AddApplication();           // MediatR, AutoMapper, Validators
builder.Services.AddDataAccess(config);      // DbContext, Repositories, UnitOfWork
builder.Services.AddInfra(config);           // File Storage, HttpClient
```

### Lifetime Management
- **Transient**: Handlers, Validators, Behaviors
- **Scoped**: DbContext, UnitOfWork, Repositories
- **Singleton**: Configuration, Logging

## Database Schema

### Relationships
```
Library (1) ──→ (Many) Folder
Folder (1) ──→ (Many) Folder (self-referencing for hierarchy)
Folder (1) ──→ (Many) Document
Document (1) ──→ (Many) DocumentVersion
User (1) ──→ (Many) AuditLog
```

### Soft Deletes
All entities have `IsDeleted` flag for soft deletes:
```csharp
// Query only active entities
var libraries = await unitOfWork.Libraries.FindAsync(l => !l.IsDeleted);
```

### Audit Trail
Every change is logged in AuditLog:
```csharp
public class AuditLog : BaseEntity
{
    public AuditActionType ActionType { get; set; }
    public string EntityType { get; set; }
    public Guid EntityId { get; set; }
    public Guid UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Description { get; set; }
}
```

## Error Handling

### Exception Hierarchy
```
Exception
  └── DomainException
      ├── EntityNotFoundException
      └── [Custom domain exceptions]
  └── ValidationException (FluentValidation)
```

### Error Response Format
```json
{
  "message": "Validation failed",
  "errors": {
    "Name": ["Library name is required"],
    "Description": ["Description must not exceed 1000 characters"]
  }
}
```

## Logging

### Structured Logging with Serilog
```csharp
Log.Information("Library created: {LibraryId} by {UserId}", libraryId, userId);
Log.Error(ex, "Failed to create library");
```

### Log Levels
- **Information**: Normal operations
- **Warning**: Potential issues
- **Error**: Errors that need attention
- **Debug**: Detailed debugging information

## Performance Considerations

### Indexes
- Foreign keys indexed for joins
- Frequently queried columns indexed
- Composite indexes for common queries

### Async Operations
- All I/O operations are async
- Prevents thread pool starvation
- Better scalability

### Lazy Loading Prevention
- Explicit includes in queries
- Projection to DTOs
- Avoid N+1 queries

## Security

### Input Validation
- FluentValidation on all commands
- Type safety with strong typing
- SQL injection prevention through EF Core

### Authentication/Authorization
- Windows Authentication for database
- HTTPS for all communications
- CORS policy configured

### Data Protection
- Soft deletes for data retention
- Audit trail for compliance
- Password hashing (future enhancement)

## Extensibility

### Adding New Features
1. Create domain entity in Domain layer
2. Create entity configuration in DataAccess layer
3. Create migration and update database
4. Create DTOs in Application layer
5. Create Commands/Queries in Application layer
6. Create Handlers and Validators in Application layer
7. Create AutoMapper profile in Application layer
8. Create Controller in API layer
9. Create Views in WebUI layer

### Adding New Services
1. Define interface in Application layer
2. Implement in Infrastructure.Infra layer
3. Register in DependencyInjection.cs
4. Inject into handlers

## Testing Strategy

### Unit Tests
- Test domain logic
- Test validators
- Test handlers with mocked repositories

### Integration Tests
- Test repositories with real DbContext
- Test handlers with real database
- Test migrations

### API Tests
- Test controllers
- Test error handling
- Test Swagger documentation

## Deployment

### Prerequisites
- .NET 8 runtime
- SQL Server (LocalDB or full)
- File storage location

### Configuration
- Update connection strings
- Configure file storage path
- Set logging levels
- Configure CORS

### Database
- Run migrations
- Seed initial data
- Verify schema

---

**This architecture ensures scalability, maintainability, and testability while following SOLID principles and industry best practices.**

