using JupiterDMS.Application.Common.Interfaces;
using JupiterDMS.Domain.Entities;
using JupiterDMS.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace JupiterDMS.Infrastructure.Infra.Services;

/// <summary>
/// Service for managing audit logs.
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuditLogService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="logger">The logger.</param>
    public AuditLogService(IUnitOfWork unitOfWork, ILogger<AuditLogService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task LogActionAsync(
        AuditActionType actionType,
        string entityType,
        Guid? entityId,
        string? entityName,
        Guid performedBy,
        string? oldValues = null,
        string? newValues = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActionType = actionType,
                EntityType = entityType,
                EntityId = entityId ?? Guid.Empty,
                UserId = performedBy,
                Timestamp = DateTime.UtcNow,
                Description = details ?? $"{actionType} performed on {entityType}",
                IpAddress = ipAddress,
                AdditionalDataJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    EntityName = entityName,
                    OldValues = oldValues,
                    NewValues = newValues,
                    UserAgent = userAgent
                }),
                CreatedOn = DateTime.UtcNow,
                CreatedBy = performedBy
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Audit log created: {ActionType} on {EntityType} {EntityId} by user {PerformedBy}",
                actionType, entityType, entityId, performedBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to create audit log: {ActionType} on {EntityType} {EntityId} by user {PerformedBy}",
                actionType, entityType, entityId, performedBy);
            
            // Don't throw - audit logging should not break the main operation
        }
    }

    /// <inheritdoc/>
    public async Task LogDocumentActionAsync(
        AuditActionType actionType,
        Guid documentId,
        string documentName,
        Guid performedBy,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        await LogActionAsync(
            actionType,
            nameof(Document),
            documentId,
            documentName,
            performedBy,
            details: details,
            ipAddress: ipAddress,
            userAgent: userAgent,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task LogFolderActionAsync(
        AuditActionType actionType,
        Guid folderId,
        string folderName,
        Guid performedBy,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        await LogActionAsync(
            actionType,
            nameof(Folder),
            folderId,
            folderName,
            performedBy,
            details: details,
            ipAddress: ipAddress,
            userAgent: userAgent,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task LogLibraryActionAsync(
        AuditActionType actionType,
        Guid libraryId,
        string libraryName,
        Guid performedBy,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        await LogActionAsync(
            actionType,
            nameof(Library),
            libraryId,
            libraryName,
            performedBy,
            details: details,
            ipAddress: ipAddress,
            userAgent: userAgent,
            cancellationToken: cancellationToken);
    }
}
