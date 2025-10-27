using JupiterDMS.Domain.Entities;
using JupiterDMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JupiterDMS.Infrastructure.DataAccess.Persistence;

/// <summary>
/// Database seeder for initial data.
/// </summary>
public class DatabaseSeeder
{
    private readonly JupiterDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSeeder"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger.</param>
    public DatabaseSeeder(JupiterDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Seeds the database with initial data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync(cancellationToken);

            // Seed demo users if not exists
            await SeedDemoUsersAsync(cancellationToken);

            // Seed default library if not exists
            await SeedDefaultLibraryAsync(cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding database");
            throw;
        }
    }

    /// <summary>
    /// Seeds demo users for different roles.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedDemoUsersAsync(CancellationToken cancellationToken)
    {
        var demoUsers = new[]
        {
            new { Username = "admin", Email = "admin@jupiterdms.com", FirstName = "System", LastName = "Administrator", Role = UserRole.Admin, Password = "Admin123!" },
            new { Username = "editor", Email = "editor@jupiterdms.com", FirstName = "John", LastName = "Editor", Role = UserRole.Editor, Password = "Editor123!" },
            new { Username = "viewer", Email = "viewer@jupiterdms.com", FirstName = "Jane", LastName = "Viewer", Role = UserRole.Viewer, Password = "Viewer123!" }
        };

        foreach (var demoUser in demoUsers)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == demoUser.Username || u.Email == demoUser.Email, cancellationToken);

            if (existingUser == null)
            {
                var userId = Guid.NewGuid();
                var user = new User
                {
                    Id = userId,
                    Username = demoUser.Username,
                    Email = demoUser.Email,
                    FirstName = demoUser.FirstName,
                    LastName = demoUser.LastName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(demoUser.Password, 12),
                    Role = demoUser.Role,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = userId // Self-created
                };

                _context.Users.Add(user);
                _logger.LogInformation("Created demo user: {Username} with role {Role}", demoUser.Username, demoUser.Role);
            }
            else
            {
                _logger.LogInformation("Demo user already exists: {Username}", existingUser.Username);
            }
        }
    }

    /// <summary>
    /// Seeds the default library.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedDefaultLibraryAsync(CancellationToken cancellationToken)
    {
        const string defaultLibraryName = "Default";

        var existingLibrary = await _context.Libraries
            .FirstOrDefaultAsync(l => l.Name == defaultLibraryName, cancellationToken);

        if (existingLibrary == null)
        {
            // Get admin user for CreatedBy
            var admin = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == UserRole.Admin, cancellationToken);

            var libraryId = Guid.NewGuid();
            var defaultLibrary = new Library
            {
                Id = libraryId,
                Name = defaultLibraryName,
                Description = "Default document library",
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = admin?.Id ?? Guid.Empty
            };

            _context.Libraries.Add(defaultLibrary);

            // Create default folders
            await SeedDefaultFoldersAsync(defaultLibrary, admin?.Id ?? Guid.Empty, cancellationToken);

            _logger.LogInformation("Created default library: {LibraryName}", defaultLibraryName);
        }
        else
        {
            _logger.LogInformation("Default library already exists: {LibraryName}", existingLibrary.Name);
        }
    }

    /// <summary>
    /// Seeds default folders in the library.
    /// </summary>
    /// <param name="library">The library to create folders in.</param>
    /// <param name="createdBy">The user ID who created the folders.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedDefaultFoldersAsync(Library library, Guid createdBy, CancellationToken cancellationToken)
    {
        var defaultFolders = new[]
        {
            "Engineering",
            "Marketing", 
            "Sales",
            "HR",
            "Finance",
            "Legal",
            "Operations"
        };

        foreach (var folderName in defaultFolders)
        {
            var existingFolder = await _context.Folders
                .FirstOrDefaultAsync(f => f.Name == folderName && f.LibraryId == library.Id, cancellationToken);

            if (existingFolder == null)
            {
                var folder = new Folder
                {
                    Id = Guid.NewGuid(),
                    Name = folderName,
                    Path = $"/{folderName}",
                    LibraryId = library.Id,
                    ParentFolderId = null, // Root level folder
                    IsDeleted = false,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = createdBy
                };

                _context.Folders.Add(folder);
                _logger.LogInformation("Created default folder: {FolderName} in library {LibraryName}", folderName, library.Name);
            }
        }
    }
}
