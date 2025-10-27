# JupiterDMS - Quick Reference Guide

## 🚀 Getting Started (5 Minutes)

### 1. Open Solution
```bash
cd JupiterDMS
code .  # or open in Visual Studio
```

### 2. Restore Packages
```bash
dotnet restore
```

### 3. Create Database
```bash
cd JupiterDMS.Infrastructure.DataAccess
dotnet ef database update
```

### 4. Run API
```bash
cd JupiterDMS.API
dotnet run
# API runs at https://localhost:7001
# Swagger at https://localhost:7001/swagger
```

### 5. Run WebUI (in another terminal)
```bash
cd JupiterDMS.WebUI
dotnet run
# WebUI runs at https://localhost:7002
```

## 📁 Project Structure

```
JupiterDMS/
├── JupiterDMS.Domain/                    # Business logic (no dependencies)
│   ├── Entities/                         # Domain entities
│   ├── Enums/                            # Enumerations
│   ├── Constants/                        # DomainConstants (no magic strings)
│   ├── Events/                           # Domain events
│   └── Exceptions/                       # Domain exceptions
│
├── JupiterDMS.Application/               # Use cases (CQRS)
│   ├── Features/Libraries/               # Example feature
│   │   ├── Commands/                     # Write operations
│   │   ├── Queries/                      # Read operations
│   │   ├── Handlers/                     # Command/Query handlers
│   │   ├── Validators/                   # FluentValidation
│   │   ├── Dtos/                         # Data transfer objects
│   │   └── Mapping/                      # AutoMapper profiles
│   ├── Common/
│   │   ├── Interfaces/                   # IRepository, IUnitOfWork
│   │   └── Behaviors/                    # Pipeline behaviors
│   └── DependencyInjection.cs            # DI registration
│
├── JupiterDMS.Infrastructure.DataAccess/ # Data access layer
│   ├── Persistence/                      # DbContext
│   ├── Configurations/                   # Entity configurations
│   ├── Repositories/                     # Repository & UnitOfWork
│   └── DependencyInjection.cs            # DI registration
│
├── JupiterDMS.Infrastructure.Infra/      # Infrastructure services
│   ├── Services/                         # FileStorageService
│   ├── Logging/                          # Serilog configuration
│   └── DependencyInjection.cs            # DI registration
│
├── JupiterDMS.API/                       # REST API
│   ├── Controllers/                      # API endpoints
│   ├── Middleware/                       # Exception handling
│   ├── Program.cs                        # Startup configuration
│   └── appsettings.json                  # Configuration
│
└── JupiterDMS.WebUI/                     # MVC Web UI
    ├── Controllers/                      # MVC controllers
    ├── Views/                            # Razor views
    ├── Services/                         # API client
    ├── Program.cs                        # Startup configuration
    └── appsettings.json                  # Configuration
```

## 🔑 Key Files

| File | Purpose |
|------|---------|
| `DomainConstants.cs` | All string constants (no magic strings) |
| `BaseEntity.cs` | Base class for all entities |
| `IRepository.cs` | Generic repository interface |
| `IUnitOfWork.cs` | Unit of Work interface |
| `GenericRepository.cs` | Generic repository implementation |
| `UnitOfWork.cs` | Unit of Work implementation |
| `JupiterDbContext.cs` | Entity Framework DbContext |
| `ValidationBehavior.cs` | MediatR validation pipeline |
| `ExceptionHandlingMiddleware.cs` | Global exception handler |
| `LibrariesController.cs` | API controller example |
| `JupiterDmsApiClient.cs` | Typed HTTP client |

## 🎯 Common Tasks

### Add a New Entity

1. **Create Entity** in `JupiterDMS.Domain/Entities/`
```csharp
public class MyEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}
```

2. **Add Constants** in `JupiterDMS.Domain/Constants/DomainConstants.cs`
```csharp
public static class MyEntity
{
    public const int NameMaxLength = 200;
}
```

3. **Create Configuration** in `JupiterDMS.Infrastructure.DataAccess/Configurations/`
```csharp
public class MyEntityConfiguration : IEntityTypeConfiguration<MyEntity>
{
    public void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        builder.ToTable("MyEntities");
        builder.Property(e => e.Name).HasMaxLength(DomainConstants.MyEntity.NameMaxLength);
    }
}
```

4. **Add DbSet** in `JupiterDbContext.cs`
```csharp
public DbSet<MyEntity> MyEntities { get; set; }
```

5. **Add Repository** in `IUnitOfWork.cs`
```csharp
IRepository<MyEntity> MyEntities { get; }
```

6. **Create Migration**
```bash
cd JupiterDMS.Infrastructure.DataAccess
dotnet ef migrations add AddMyEntity
dotnet ef database update
```

### Add a New Command

1. **Create Command** in `JupiterDMS.Application/Features/MyFeature/Commands/`
```csharp
public class CreateMyEntityCommand : IRequest<MyEntityDto>
{
    public string Name { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
}
```

2. **Create Validator** in `JupiterDMS.Application/Features/MyFeature/Validators/`
```csharp
public class CreateMyEntityCommandValidator : AbstractValidator<CreateMyEntityCommand>
{
    public CreateMyEntityCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(DomainConstants.MyEntity.NameMaxLength);
    }
}
```

3. **Create Handler** in `JupiterDMS.Application/Features/MyFeature/Handlers/`
```csharp
public class CreateMyEntityCommandHandler : IRequestHandler<CreateMyEntityCommand, MyEntityDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateMyEntityCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MyEntityDto> Handle(CreateMyEntityCommand request, CancellationToken cancellationToken)
    {
        var entity = new MyEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _unitOfWork.MyEntities.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MyEntityDto>(entity);
    }
}
```

### Add a New Query

1. **Create Query** in `JupiterDMS.Application/Features/MyFeature/Queries/`
```csharp
public class GetMyEntityByIdQuery : IRequest<MyEntityDto?>
{
    public Guid Id { get; set; }
}
```

2. **Create Handler** in `JupiterDMS.Application/Features/MyFeature/Handlers/`
```csharp
public class GetMyEntityByIdQueryHandler : IRequestHandler<GetMyEntityByIdQuery, MyEntityDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMyEntityByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MyEntityDto?> Handle(GetMyEntityByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.MyEntities.GetByIdAsync(request.Id, cancellationToken);
        return entity == null ? null : _mapper.Map<MyEntityDto>(entity);
    }
}
```

### Add an API Endpoint

1. **Create Controller** in `JupiterDMS.API/Controllers/`
```csharp
[ApiController]
[Route("api/[controller]")]
public class MyEntitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MyEntitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<MyEntityDto>> Create(
        CreateMyEntityCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MyEntityDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMyEntityByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }
}
```

## 🧪 Testing

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
dotnet test JupiterDMS.Application.Tests
```

### Run with Coverage
```bash
dotnet test /p:CollectCoverage=true
```

## 📊 Database Commands

### Create Migration
```bash
cd JupiterDMS.Infrastructure.DataAccess
dotnet ef migrations add MigrationName
```

### Update Database
```bash
dotnet ef database update
```

### Remove Last Migration
```bash
dotnet ef migrations remove
```

### Drop Database
```bash
dotnet ef database drop
```

### View SQL
```bash
dotnet ef migrations script
```

## 🔍 Debugging

### Enable Detailed Logging
Edit `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  }
}
```

### View SQL Queries
Add to `JupiterDbContext.OnConfiguring()`:
```csharp
optionsBuilder.LogTo(Console.WriteLine);
```

### Breakpoints
- Set breakpoints in handlers
- Use Debug > Start Debugging (F5)
- Step through code with F10/F11

## 📝 Code Snippets

### Repository Query
```csharp
var items = await _unitOfWork.MyEntities.FindAsync(
    x => x.IsActive && !x.IsDeleted,
    cancellationToken);
```

### Check Existence
```csharp
var exists = await _unitOfWork.MyEntities.AnyAsync(
    x => x.Id == id && !x.IsDeleted,
    cancellationToken);
```

### Count Records
```csharp
var count = await _unitOfWork.MyEntities.CountAsync(
    x => !x.IsDeleted,
    cancellationToken);
```

### Soft Delete
```csharp
entity.IsDeleted = true;
entity.ModifiedOn = DateTime.UtcNow;
entity.ModifiedBy = userId;
_unitOfWork.MyEntities.Update(entity);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

### Transaction
```csharp
await _unitOfWork.BeginTransactionAsync(cancellationToken);
try
{
    // Do work
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    await _unitOfWork.CommitTransactionAsync(cancellationToken);
}
catch
{
    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
    throw;
}
```

## 🐛 Common Issues

| Issue | Solution |
|-------|----------|
| "DbContext has been disposed" | Ensure async/await is used correctly |
| "Validation failed" | Check validator rules match command properties |
| "Entity not found" | Verify entity exists before operations |
| "Connection string error" | Check appsettings.json and LocalDB installation |
| "Migration pending" | Run `dotnet ef database update` |
| "Circular reference in AutoMapper" | Create separate DTOs or use `.ForMember()` |

## 📚 Documentation Files

- **README.md** - Project overview
- **SETUP_GUIDE.md** - Installation instructions
- **ARCHITECTURE.md** - Architecture details
- **ARCHITECTURE_DIAGRAM.md** - Visual diagrams
- **DEVELOPMENT_GUIDE.md** - Development patterns
- **QUICK_REFERENCE.md** - This file
- **PROJECT_SUMMARY.md** - Project statistics

## 🔗 Useful Links

- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [MediatR](https://github.com/jbogard/MediatR)
- [FluentValidation](https://fluentvalidation.net/)
- [AutoMapper](https://automapper.org/)
- [Serilog](https://serilog.net/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://www.domainlanguage.com/ddd/)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)

## 💡 Tips & Tricks

1. **Use Constants** - Always use `DomainConstants` instead of magic strings
2. **Async Everywhere** - All I/O operations should be async
3. **Validation First** - Validate input before processing
4. **Soft Deletes** - Use `IsDeleted` flag instead of hard deletes
5. **Audit Trail** - Log all important operations
6. **Error Handling** - Let middleware handle exceptions
7. **DTOs** - Always map entities to DTOs before returning
8. **Transactions** - Use Unit of Work for multi-step operations
9. **Logging** - Use Serilog for structured logging
10. **Documentation** - Add XML comments to public APIs

## 🎓 Learning Path

1. **Understand Architecture** - Read ARCHITECTURE.md
2. **Review Domain Layer** - Study entities and constants
3. **Study CQRS** - Review Libraries feature
4. **Explore Handlers** - Understand command/query handlers
5. **Check Validators** - See FluentValidation usage
6. **Review API** - Study LibrariesController
7. **Add New Feature** - Follow DEVELOPMENT_GUIDE.md
8. **Write Tests** - Create unit and integration tests

---

**Happy coding! 🚀**

