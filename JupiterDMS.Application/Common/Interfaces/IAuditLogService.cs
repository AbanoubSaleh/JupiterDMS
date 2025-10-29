using JupiterDMS.Domain.Enums;

namespace JupiterDMS.Application.Common.Interfaces;

/// <summary>
/// Service for managing audit logs.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Logs an audit action asynchronously.
    /// </summary>
    /// <param name="actionType">The type of action performed.</param>
    /// <param name="entityType">The type of entity affected.</param>
    /// <param name="entityId">The identifier of the entity affected.</param>
    /// <param name="entityName">The name of the entity affected.</param>
    /// <param name="performedBy">The identifier of the user who performed the action.</param>
    /// <param name="oldValues">The old values before the action (JSON).</param>
    /// <param name="newValues">The new values after the action (JSON).</param>
    /// <param name="details">Additional details about the action.</param>
    /// <param name="ipAddress">The IP address of the user.</param>
    /// <param name="userAgent">The user agent string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogActionAsync(
        AuditActionType actionType,
        string entityType,
        Guid? entityId,
        string? entityName,
        string performedBy,
        string? oldValues = null,
        string? newValues = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a document action asynchronously.
    /// </summary>
    /// <param name="actionType">The type of action performed.</param>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="documentName">The document name.</param>
    /// <param name="performedBy">The identifier of the user who performed the action.</param>
    /// <param name="details">Additional details about the action.</param>
    /// <param name="ipAddress">The IP address of the user.</param>
    /// <param name="userAgent">The user agent string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogDocumentActionAsync(
        AuditActionType actionType,
        Guid documentId,
        string documentName,
        string performedBy,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a folder action asynchronously.
    /// </summary>
    /// <param name="actionType">The type of action performed.</param>
    /// <param name="folderId">The folder identifier.</param>
    /// <param name="folderName">The folder name.</param>
    /// <param name="performedBy">The identifier of the user who performed the action.</param>
    /// <param name="details">Additional details about the action.</param>
    /// <param name="ipAddress">The IP address of the user.</param>
    /// <param name="userAgent">The user agent string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogFolderActionAsync(
        AuditActionType actionType,
        Guid folderId,
        string folderName,
        string performedBy,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a library action asynchronously.
    /// </summary>
    /// <param name="actionType">The type of action performed.</param>
    /// <param name="libraryId">The library identifier.</param>
    /// <param name="libraryName">The library name.</param>
    /// <param name="performedBy">The identifier of the user who performed the action.</param>
    /// <param name="details">Additional details about the action.</param>
    /// <param name="ipAddress">The IP address of the user.</param>
    /// <param name="userAgent">The user agent string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogLibraryActionAsync(
        AuditActionType actionType,
        Guid libraryId,
        string libraryName,
        string performedBy,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
}
