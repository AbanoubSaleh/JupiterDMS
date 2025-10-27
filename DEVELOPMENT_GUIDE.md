# JupiterDMS Development Guide

## Adding a New Feature

This guide walks through adding a new feature following the established patterns.

### Example: Adding Folder Management

#### Step 1: Create Domain Entity

**File**: `JupiterDMS.Domain/Entities/Folder.cs`

```csharp
public class Folder : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public Guid LibraryId { get; set; }
    public Guid? ParentFolderId { get; set; }
    
    public virtual Library Library { get; set; } = null!;
    public virtual Folder? ParentFolder { get; set; }
    public virtual ICollection<Folder> ChildFolders { get; set; } = new List<Folder>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
```

#### Step 2: Add Domain Constants

**File**: `JupiterDMS.Domain/Constants/DomainConstants.cs`

```csharp
public static class Folder
{
    public const int NameMaxLength = 200;
    public const int PathMaxLength = 2000;
}
```

#### Step 3: Create Entity Configuration

**File**: `JupiterDMS.Infrastructure.DataAccess/Configurations/FolderConfiguration.cs`

```csharp
public class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.ToTable("Folders");
        builder.HasKey(f => f.Id);
        
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(DomainConstants.Folder.NameMaxLength);
        
        // Configure relationships
        builder.HasOne(f => f.Library)
            .WithMany(l => l.Folders)
            .HasForeignKey(f => f.LibraryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

#### Step 4: Create DTOs

**File**: `JupiterDMS.Application/Features/Folders/Dtos/FolderDto.cs`

```csharp
public class FolderDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public Guid LibraryId { get; set; }
    public Guid? ParentFolderId { get; set; }
    public DateTime CreatedOn { get; set; }
}
```

#### Step 5: Create CQRS Commands

**File**: `JupiterDMS.Application/Features/Folders/Commands/CreateFolderCommand.cs`

```csharp
public class CreateFolderCommand : IRequest<FolderDto>
{
    public string Name { get; set; } = string.Empty;
    public Guid LibraryId { get; set; }
    public Guid? ParentFolderId { get; set; }
    public Guid CreatedBy { get; set; }
}
```

#### Step 6: Create Validators

**File**: `JupiterDMS.Application/Features/Folders/Validators/CreateFolderCommandValidator.cs`

```csharp
public class CreateFolderCommandValidator : AbstractValidator<CreateFolderCommand>
{
    public CreateFolderCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Folder name is required.")
            .MaximumLength(DomainConstants.Folder.NameMaxLength);
        
        RuleFor(x => x.LibraryId)
            .NotEmpty().WithMessage("Library ID is required.");
    }
}
```

#### Step 7: Create Handlers

**File**: `JupiterDMS.Application/Features/Folders/Handlers/CreateFolderCommandHandler.cs`

```csharp
public class CreateFolderCommandHandler : IRequestHandler<CreateFolderCommand, FolderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateFolderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<FolderDto> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
    {
        // Validate library exists
        var library = await _unitOfWork.Libraries.GetByIdAsync(request.LibraryId, cancellationToken);
        if (library == null)
            throw new EntityNotFoundException(nameof(Library), request.LibraryId);

        var folder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            LibraryId = request.LibraryId,
            ParentFolderId = request.ParentFolderId,
            Path = GeneratePath(request.Name, request.ParentFolderId),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = request.CreatedBy,
            IsDeleted = false
        };

        await _unitOfWork.Folders.AddAsync(folder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FolderDto>(folder);
    }

    private string GeneratePath(string name, Guid? parentId)
    {
        // Generate hierarchical path
        return parentId.HasValue ? $"{parentId}/{name}" : $"/{name}";
    }
}
```

#### Step 8: Create AutoMapper Profile

**File**: `JupiterDMS.Application/Features/Folders/Mapping/FolderMappingProfile.cs`

```csharp
public class FolderMappingProfile : Profile
{
    public FolderMappingProfile()
    {
        CreateMap<Folder, FolderDto>();
        CreateMap<FolderDto, Folder>();
    }
}
```

#### Step 9: Create API Controller

**File**: `JupiterDMS.API/Controllers/FoldersController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
public class FoldersController : ControllerBase
{
    private readonly IMediator _mediator;

    public FoldersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FolderDto>> Create(
        CreateFolderCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FolderDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFolderByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }
}
```

#### Step 10: Create Database Migration

```bash
cd JupiterDMS.Infrastructure.DataAccess
dotnet ef migrations add AddFolderEntity
dotnet ef database update
```

## Best Practices

### 1. Always Use Constants

❌ **Bad**:
```csharp
if (name.Length > 200)
    throw new ValidationException("Name too long");
```

✅ **Good**:
```csharp
if (name.Length > DomainConstants.Folder.NameMaxLength)
    throw new ValidationException($"Name must not exceed {DomainConstants.Folder.NameMaxLength} characters");
```

### 2. Use Async/Await Everywhere

❌ **Bad**:
```csharp
public FolderDto GetFolder(Guid id)
{
    return _unitOfWork.Folders.GetById(id);
}
```

✅ **Good**:
```csharp
public async Task<FolderDto> GetFolderAsync(Guid id, CancellationToken cancellationToken = default)
{
    return await _unitOfWork.Folders.GetByIdAsync(id, cancellationToken);
}
```

### 3. Add XML Documentation

❌ **Bad**:
```csharp
public async Task<FolderDto> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
{
    // ...
}
```

✅ **Good**:
```csharp
/// <summary>
/// Handles the create folder command.
/// </summary>
/// <param name="request">The create folder command.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The created folder DTO.</returns>
public async Task<FolderDto> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
{
    // ...
}
```

### 4. Validate Before Processing

❌ **Bad**:
```csharp
public async Task<FolderDto> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
{
    var folder = new Folder { Name = request.Name };
    // ... process
}
```

✅ **Good**:
```csharp
public async Task<FolderDto> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
{
    var library = await _unitOfWork.Libraries.GetByIdAsync(request.LibraryId, cancellationToken);
    if (library == null)
        throw new EntityNotFoundException(nameof(Library), request.LibraryId);
    
    var folder = new Folder { Name = request.Name };
    // ... process
}
```

### 5. Use Proper HTTP Status Codes

❌ **Bad**:
```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateFolderCommand command)
{
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

✅ **Good**:
```csharp
[HttpPost]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<FolderDto>> Create(CreateFolderCommand command)
{
    var result = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}
```

### 6. Handle Exceptions Properly

❌ **Bad**:
```csharp
try
{
    // ... code
}
catch (Exception ex)
{
    return BadRequest("Error");
}
```

✅ **Good**:
```csharp
// Let middleware handle exceptions
// Throw specific domain exceptions
throw new EntityNotFoundException(nameof(Library), libraryId);
```

### 7. Use Dependency Injection

❌ **Bad**:
```csharp
public class CreateFolderCommandHandler
{
    private readonly IUnitOfWork _unitOfWork = new UnitOfWork();
}
```

✅ **Good**:
```csharp
public class CreateFolderCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateFolderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}
```

### 8. Separate Concerns

❌ **Bad**:
```csharp
public class FoldersController : ControllerBase
{
    public async Task<IActionResult> Create(CreateFolderCommand command)
    {
        var folder = new Folder { Name = command.Name };
        _context.Folders.Add(folder);
        await _context.SaveChangesAsync();
        return Ok(folder);
    }
}
```

✅ **Good**:
```csharp
public class FoldersController : ControllerBase
{
    private readonly IMediator _mediator;

    public async Task<ActionResult<FolderDto>> Create(CreateFolderCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
```

## Code Review Checklist

- [ ] No magic strings (use constants)
- [ ] All public methods have XML documentation
- [ ] All I/O operations are async
- [ ] Proper exception handling
- [ ] Validators for all commands
- [ ] AutoMapper profiles for DTOs
- [ ] Proper HTTP status codes
- [ ] Database migration created
- [ ] Entity configuration added
- [ ] Unit tests written
- [ ] Integration tests written

## Common Patterns

### Querying with Filtering

```csharp
var folders = await _unitOfWork.Folders.FindAsync(
    f => f.LibraryId == libraryId && !f.IsDeleted,
    cancellationToken);
```

### Checking Existence

```csharp
var exists = await _unitOfWork.Folders.AnyAsync(
    f => f.Id == folderId && !f.IsDeleted,
    cancellationToken);
```

### Counting Records

```csharp
var count = await _unitOfWork.Folders.CountAsync(
    f => f.LibraryId == libraryId && !f.IsDeleted,
    cancellationToken);
```

### Soft Delete

```csharp
folder.IsDeleted = true;
folder.ModifiedOn = DateTime.UtcNow;
folder.ModifiedBy = userId;
_unitOfWork.Folders.Update(folder);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

## Troubleshooting

### Issue: "Validation failed" error

**Solution**: Check the validator rules and ensure the command properties match the validation rules.

### Issue: "Entity not found" error

**Solution**: Verify the entity exists in the database before performing operations.

### Issue: "DbContext has been disposed" error

**Solution**: Ensure you're using async/await properly and not disposing the context prematurely.

### Issue: "Circular reference" in AutoMapper

**Solution**: Create separate DTOs for nested objects or use `.ForMember()` to exclude circular references.

---

**Follow these patterns and best practices to maintain code quality and consistency across the project.**

