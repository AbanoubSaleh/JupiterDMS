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

            // Seed admin user if not exists
            await SeedAdminUserAsync(cancellationToken);

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
    /// Seeds the default admin user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        const string adminUsername = "admin";
        const string adminEmail = "admin@jupiterdms.com";

        var existingAdmin = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == adminUsername || u.Email == adminEmail, cancellationToken);

        if (existingAdmin == null)
        {
            var adminId = Guid.NewGuid();
            var admin = new User
            {
                Id = adminId,
                Username = adminUsername,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", 12), // Default password
                Role = UserRole.Admin,
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = adminId // Self-created
            };

            _context.Users.Add(admin);
            _logger.LogInformation("Created default admin user: {Username}", adminUsername);
        }
        else
        {
            _logger.LogInformation("Admin user already exists: {Username}", existingAdmin.Username);
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
