# JupiterDMS Architecture Diagrams

## System Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         CLIENT LAYER                             │
├─────────────────────────────────────────────────────────────────┤
│  Web Browser (WebUI)          │  Mobile App / Desktop Client     │
│  https://localhost:7002       │  https://localhost:7001/api      │
└──────────────┬────────────────┴──────────────┬───────────────────┘
               │                               │
               │ HTTP/HTTPS                    │ HTTP/HTTPS
               │                               │
┌──────────────▼───────────────────────────────▼───────────────────┐
│                    PRESENTATION LAYER                             │
├─────────────────────────────────────────────────────────────────┤
│  JupiterDMS.WebUI (MVC)       │  JupiterDMS.API (REST)          │
│  ├─ Controllers               │  ├─ Controllers                  │
│  ├─ Views (Razor)             │  ├─ Middleware                   │
│  ├─ Services (HttpClient)     │  ├─ Swagger/OpenAPI              │
│  └─ Models                    │  └─ Health Checks                │
└──────────────┬────────────────┴──────────────┬───────────────────┘
               │                               │
               └───────────────┬───────────────┘
                               │
                    MediatR.Send(Command/Query)
                               │
┌──────────────────────────────▼───────────────────────────────────┐
│                  APPLICATION LAYER                               │
├─────────────────────────────────────────────────────────────────┤
│  JupiterDMS.Application                                          │
│  ├─ Features/                                                    │
│  │  └─ Libraries/                                               │
│  │     ├─ Commands (Create, Update, Delete)                    │
│  │     ├─ Queries (GetAll, GetById)                            │
│  │     ├─ Handlers (Command/Query Handlers)                    │
│  │     ├─ Validators (FluentValidation)                        │
│  │     ├─ Dtos (LibraryDto)                                    │
│  │     └─ Mapping (AutoMapper Profiles)                        │
│  ├─ Common/                                                      │
│  │  ├─ Interfaces (IRepository, IUnitOfWork, IFileStorage)     │
│  │  └─ Behaviors (ValidationBehavior)                          │
│  └─ DependencyInjection.cs                                      │
└──────────────┬────────────────────────────────────────────────────┘
               │
               │ IUnitOfWork.SaveChangesAsync()
               │
┌──────────────▼────────────────────────────────────────────────────┐
│              INFRASTRUCTURE.DATAACCESS LAYER                       │
├────────────────────────────────────────────────────────────────────┤
│  JupiterDMS.Infrastructure.DataAccess                              │
│  ├─ Persistence/                                                   │
│  │  └─ JupiterDbContext                                           │
│  ├─ Configurations/                                                │
│  │  ├─ LibraryConfiguration                                       │
│  │  ├─ FolderConfiguration                                        │
│  │  ├─ DocumentConfiguration                                      │
│  │  ├─ DocumentVersionConfiguration                               │
│  │  ├─ UserConfiguration                                          │
│  │  └─ AuditLogConfiguration                                      │
│  ├─ Repositories/                                                  │
│  │  ├─ GenericRepository<T>                                       │
│  │  └─ UnitOfWork                                                 │
│  └─ DependencyInjection.cs                                        │
└──────────────┬────────────────────────────────────────────────────┘
               │
               │ DbContext.SaveChangesAsync()
               │
┌──────────────▼────────────────────────────────────────────────────┐
│                    DATABASE LAYER                                  │
├────────────────────────────────────────────────────────────────────┤
│  SQL Server LocalDB (Windows Authentication)                       │
│  ├─ Libraries Table                                                │
│  ├─ Folders Table                                                  │
│  ├─ Documents Table                                                │
│  ├─ DocumentVersions Table                                         │
│  ├─ Users Table                                                    │
│  └─ AuditLogs Table                                                │
└────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│              INFRASTRUCTURE.INFRA LAYER                           │
├──────────────────────────────────────────────────────────────────┤
│  JupiterDMS.Infrastructure.Infra                                  │
│  ├─ Services/                                                     │
│  │  └─ FileStorageService (IFileStorageService)                 │
│  ├─ Logging/                                                      │
│  │  └─ SerilogConfiguration                                       │
│  └─ DependencyInjection.cs                                        │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│                    DOMAIN LAYER                                   │
├──────────────────────────────────────────────────────────────────┤
│  JupiterDMS.Domain (Pure Business Logic)                          │
│  ├─ Entities/                                                     │
│  │  ├─ Library                                                    │
│  │  ├─ Folder                                                     │
│  │  ├─ Document                                                   │
│  │  ├─ DocumentVersion                                            │
│  │  ├─ User                                                       │
│  │  └─ AuditLog                                                   │
│  ├─ Common/                                                       │
│  │  ├─ BaseEntity                                                 │
│  │  └─ IDomainEvent                                               │
│  ├─ Enums/                                                        │
│  │  ├─ UserRole                                                   │
│  │  ├─ AuditActionType                                            │
│  │  └─ DocumentStatus                                             │
│  ├─ Constants/                                                    │
│  │  └─ DomainConstants                                            │
│  ├─ Events/                                                       │
│  │  ├─ DocumentUploadedEvent                                      │
│  │  └─ LibraryCreatedEvent                                        │
│  └─ Exceptions/                                                   │
│     ├─ DomainException                                            │
│     └─ EntityNotFoundException                                    │
└──────────────────────────────────────────────────────────────────┘
```

## CQRS Command Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    HTTP POST Request                             │
│              POST /api/libraries                                 │
│         { "name": "My Library", ... }                            │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│              LibrariesController.Create()                        │
│                                                                  │
│  public async Task<ActionResult<LibraryDto>> Create(            │
│      CreateLibraryCommand command)                              │
│  {                                                               │
│      var result = await _mediator.Send(command);               │
│      return CreatedAtAction(...);                              │
│  }                                                               │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│              MediatR Pipeline                                    │
│                                                                  │
│  1. ValidationBehavior                                           │
│     ├─ CreateLibraryCommandValidator                            │
│     └─ Validates: Name, Description, CreatedBy                 │
│                                                                  │
│  2. CreateLibraryCommandHandler                                 │
│     ├─ Create Library entity                                    │
│     ├─ unitOfWork.Libraries.AddAsync()                          │
│     └─ unitOfWork.SaveChangesAsync()                            │
│                                                                  │
│  3. AutoMapper                                                   │
│     └─ Library → LibraryDto                                     │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│              Database Operation                                  │
│                                                                  │
│  DbContext.SaveChangesAsync()                                    │
│  ├─ INSERT INTO Libraries (Id, Name, Description, ...)         │
│  └─ Commit Transaction                                          │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│              HTTP 201 Created Response                           │
│                                                                  │
│  {                                                               │
│    "id": "guid",                                                │
│    "name": "My Library",                                        │
│    "description": "...",                                        │
│    "isActive": true,                                            │
│    "createdOn": "2024-01-01T00:00:00Z"                         │
│  }                                                               │
└─────────────────────────────────────────────────────────────────┘
```

## CQRS Query Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    HTTP GET Request                              │
│              GET /api/libraries                                  │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│              LibrariesController.GetAll()                        │
│                                                                  │
│  public async Task<ActionResult<IEnumerable<LibraryDto>>>       │
│      GetAll(bool includeInactive = false)                       │
│  {                                                               │
│      var query = new GetAllLibrariesQuery                       │
│          { IncludeInactive = includeInactive };                 │
│      var result = await _mediator.Send(query);                 │
│      return Ok(result);                                         │
│  }                                                               │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│              GetAllLibrariesQueryHandler                         │
│                                                                  │
│  public async Task<IEnumerable<LibraryDto>> Handle(...)         │
│  {                                                               │
│      var libraries = await _unitOfWork.Libraries.FindAsync(     │
│          l => !l.IsDeleted && (includeInactive || l.IsActive)  │
│      );                                                          │
│      return _mapper.Map<IEnumerable<LibraryDto>>(libraries);   │
│  }                                                               │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│              Database Query                                      │
│                                                                  │
│  SELECT * FROM Libraries                                        │
│  WHERE IsDeleted = 0 AND (IsActive = 1 OR @includeInactive)    │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│              AutoMapper Mapping                                  │
│                                                                  │
│  Library[] → LibraryDto[]                                        │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│              HTTP 200 OK Response                                │
│                                                                  │
│  [                                                               │
│    {                                                             │
│      "id": "guid1",                                             │
│      "name": "Library 1",                                       │
│      ...                                                         │
│    },                                                            │
│    {                                                             │
│      "id": "guid2",                                             │
│      "name": "Library 2",                                       │
│      ...                                                         │
│    }                                                             │
│  ]                                                               │
└─────────────────────────────────────────────────────────────────┘
```

## Dependency Injection Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    Program.cs                                    │
│                                                                  │
│  var builder = WebApplicationBuilder.CreateBuilder(args);       │
│                                                                  │
│  // Register all layers                                         │
│  builder.Services.AddApplication();                             │
│  builder.Services.AddDataAccess(builder.Configuration);         │
│  builder.Services.AddInfra(builder.Configuration);              │
│                                                                  │
│  var app = builder.Build();                                     │
│  await app.RunAsync();                                          │
└─────────────────────────────────────────────────────────────────┘
                     │
        ┌────────────┼────────────┐
        │            │            │
        ▼            ▼            ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│ Application  │ │ DataAccess   │ │ Infra        │
│ Layer        │ │ Layer        │ │ Layer        │
├──────────────┤ ├──────────────┤ ├──────────────┤
│ • MediatR    │ │ • DbContext  │ │ • File       │
│ • AutoMapper │ │ • Repos      │ │   Storage    │
│ • Validators │ │ • UnitOfWork │ │ • Logging    │
│ • Behaviors  │ │              │ │ • HttpClient │
└──────────────┘ └──────────────┘ └──────────────┘
        │            │            │
        └────────────┼────────────┘
                     │
                     ▼
        ┌────────────────────────┐
        │   Service Container    │
        │                        │
        │ Resolves dependencies  │
        │ Injects into handlers  │
        │ Manages lifetimes      │
        └────────────────────────┘
```

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                  │
│  ┌──────────────┐                                               │
│  │   Library    │                                               │
│  ├──────────────┤                                               │
│  │ Id (PK)      │                                               │
│  │ Name         │                                               │
│  │ Description  │                                               │
│  │ IsActive     │                                               │
│  │ CreatedOn    │                                               │
│  │ CreatedBy    │                                               │
│  │ IsDeleted    │                                               │
│  └──────────────┘                                               │
│         │ (1)                                                    │
│         │ HasMany                                                │
│         │                                                        │
│         ▼ (Many)                                                 │
│  ┌──────────────┐                                               │
│  │   Folder     │                                               │
│  ├──────────────┤                                               │
│  │ Id (PK)      │                                               │
│  │ Name         │                                               │
│  │ Path         │                                               │
│  │ LibraryId(FK)│◄─────────────────────────────────┐            │
│  │ ParentId(FK) │◄──────────────────┐              │            │
│  │ CreatedOn    │                   │              │            │
│  │ CreatedBy    │                   │              │            │
│  │ IsDeleted    │                   │              │            │
│  └──────────────┘                   │              │            │
│         │ (1)                       │ (Self-Ref)   │            │
│         │ HasMany                   │              │            │
│         │                           │              │            │
│         ▼ (Many)                    │              │            │
│  ┌──────────────┐                   │              │            │
│  │  Document    │                   │              │            │
│  ├──────────────┤                   │              │            │
│  │ Id (PK)      │                   │              │            │
│  │ Name         │                   │              │            │
│  │ FilePath     │                   │              │            │
│  │ FolderId(FK) │◄──────────────────┘              │            │
│  │ CurrentVer   │                                  │            │
│  │ Status       │                                  │            │
│  │ CreatedOn    │                                  │            │
│  │ CreatedBy    │                                  │            │
│  │ IsDeleted    │                                  │            │
│  └──────────────┘                                  │            │
│         │ (1)                                      │            │
│         │ HasMany                                  │            │
│         │                                          │            │
│         ▼ (Many)                                   │            │
│  ┌──────────────────┐                             │            │
│  │ DocumentVersion  │                             │            │
│  ├──────────────────┤                             │            │
│  │ Id (PK)          │                             │            │
│  │ DocumentId (FK)  │◄────────────────────────────┘            │
│  │ VersionNumber    │                                          │
│  │ FilePath         │                                          │
│  │ FileHash         │                                          │
│  │ Comment          │                                          │
│  │ CreatedOn        │                                          │
│  │ CreatedBy        │                                          │
│  │ IsDeleted        │                                          │
│  └──────────────────┘                                          │
│                                                                  │
│  ┌──────────────┐                                               │
│  │    User      │                                               │
│  ├──────────────┤                                               │
│  │ Id (PK)      │                                               │
│  │ Username     │                                               │
│  │ Email        │                                               │
│  │ PasswordHash │                                               │
│  │ FirstName    │                                               │
│  │ LastName     │                                               │
│  │ Role         │                                               │
│  │ IsActive     │                                               │
│  │ CreatedOn    │                                               │
│  │ CreatedBy    │                                               │
│  │ IsDeleted    │                                               │
│  └──────────────┘                                               │
│         │ (1)                                                    │
│         │ HasMany                                                │
│         │                                                        │
│         ▼ (Many)                                                 │
│  ┌──────────────┐                                               │
│  │  AuditLog    │                                               │
│  ├──────────────┤                                               │
│  │ Id (PK)      │                                               │
│  │ ActionType   │                                               │
│  │ EntityType   │                                               │
│  │ EntityId     │                                               │
│  │ UserId (FK)  │◄──────────────────────────────────────────────┤
│  │ Timestamp    │                                               │
│  │ Description  │                                               │
│  │ IpAddress    │                                               │
│  │ CreatedOn    │                                               │
│  │ CreatedBy    │                                               │
│  │ IsDeleted    │                                               │
│  └──────────────┘                                               │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

**These diagrams illustrate the complete architecture and data flow of JupiterDMS.**

