using JupiterDMS.Domain.Entities;

namespace JupiterDMS.Application.Common.Interfaces;

/// <summary>
/// Unit of Work interface for managing database transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the library repository.
    /// </summary>
    IRepository<Library> Libraries { get; }

    /// <summary>
    /// Gets the folder repository.
    /// </summary>
    IRepository<Folder> Folders { get; }

    /// <summary>
    /// Gets the document repository.
    /// </summary>
    IRepository<Document> Documents { get; }

    /// <summary>
    /// Gets the document version repository.
    /// </summary>
    IRepository<DocumentVersion> DocumentVersions { get; }

    /// <summary>
    /// Gets the user repository.
    /// </summary>
    IRepository<User> Users { get; }

    /// <summary>
    /// Gets the audit log repository.
    /// </summary>
    IRepository<AuditLog> AuditLogs { get; }

    /// <summary>
    /// Saves all changes made in this unit of work asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current database transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current database transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

