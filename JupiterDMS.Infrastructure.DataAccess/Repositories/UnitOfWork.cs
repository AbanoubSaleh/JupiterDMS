using JupiterDMS.Application.Common.Interfaces;
using JupiterDMS.Domain.Entities;
using JupiterDMS.Infrastructure.DataAccess.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace JupiterDMS.Infrastructure.DataAccess.Repositories;

/// <summary>
/// Unit of Work implementation for managing database transactions.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly JupiterDbContext _context;
    private IDbContextTransaction? _transaction;

    private IRepository<Library>? _libraries;
    private IRepository<Folder>? _folders;
    private IRepository<Document>? _documents;
    private IRepository<DocumentVersion>? _documentVersions;
    private IRepository<User>? _users;
    private IRepository<AuditLog>? _auditLogs;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UnitOfWork(JupiterDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public IRepository<Library> Libraries =>
        _libraries ??= new GenericRepository<Library>(_context);

    /// <inheritdoc/>
    public IRepository<Folder> Folders =>
        _folders ??= new GenericRepository<Folder>(_context);

    /// <inheritdoc/>
    public IRepository<Document> Documents =>
        _documents ??= new GenericRepository<Document>(_context);

    /// <inheritdoc/>
    public IRepository<DocumentVersion> DocumentVersions =>
        _documentVersions ??= new GenericRepository<DocumentVersion>(_context);

    /// <inheritdoc/>
    public IRepository<User> Users =>
        _users ??= new GenericRepository<User>(_context);

    /// <inheritdoc/>
    public IRepository<AuditLog> AuditLogs =>
        _auditLogs ??= new GenericRepository<AuditLog>(_context);

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <inheritdoc/>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Disposes the unit of work and releases resources.
    /// </summary>
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}

