using JupiterDMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JupiterDMS.Infrastructure.DataAccess.Persistence;

/// <summary>
/// Database context for JupiterDMS application.
/// </summary>
public class JupiterDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JupiterDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public JupiterDbContext(DbContextOptions<JupiterDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Libraries DbSet.
    /// </summary>
    public DbSet<Library> Libraries { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Folders DbSet.
    /// </summary>
    public DbSet<Folder> Folders { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Documents DbSet.
    /// </summary>
    public DbSet<Document> Documents { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DocumentVersions DbSet.
    /// </summary>
    public DbSet<DocumentVersion> DocumentVersions { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Users DbSet.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Gets or sets the AuditLogs DbSet.
    /// </summary>
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    /// <summary>
    /// Configures the model using Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JupiterDbContext).Assembly);
    }
}

