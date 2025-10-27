using JupiterDMS.Application.Common.DTOs;
using JupiterDMS.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace JupiterDMS.Application.Services;

/// <summary>
/// Interface for document management operations.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Gets all documents in a folder.
    /// </summary>
    /// <param name="folderId">The folder ID.</param>
    /// <param name="includeDeleted">Whether to include deleted documents.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of documents.</returns>
    Task<IEnumerable<DocumentDto>> GetDocumentsByFolderAsync(Guid folderId, bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a document by ID.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document information.</returns>
    Task<DocumentDto?> GetDocumentByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a new document.
    /// </summary>
    /// <param name="request">The document upload request.</param>
    /// <param name="file">The uploaded file.</param>
    /// <param name="uploadedBy">The user ID who uploaded the document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created document information.</returns>
    Task<DocumentDto> UploadDocumentAsync(UploadDocumentDto request, IFormFile file, Guid uploadedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates document metadata.
    /// </summary>
    /// <param name="request">The document update request.</param>
    /// <param name="updatedBy">The user ID who updated the document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated document information.</returns>
    Task<DocumentDto> UpdateDocumentAsync(UpdateDocumentDto request, Guid updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a document.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document download information.</returns>
    Task<DocumentDownloadDto> DownloadDocumentAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a document (soft delete).
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="deletedBy">The user ID who deleted the document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the document was deleted successfully, false otherwise.</returns>
    Task<bool> DeleteDocumentAsync(Guid id, Guid deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a document to a different folder.
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="targetFolderId">The target folder ID.</param>
    /// <param name="movedBy">The user ID who moved the document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated document information.</returns>
    Task<DocumentDto> MoveDocumentAsync(Guid documentId, Guid targetFolderId, Guid movedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new version of a document.
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="file">The new file version.</param>
    /// <param name="versionComment">The version comment.</param>
    /// <param name="uploadedBy">The user ID who uploaded the new version.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated document information.</returns>
    Task<DocumentDto> CreateDocumentVersionAsync(Guid documentId, IFormFile file, string? versionComment, Guid uploadedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all versions of a document.
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of document versions.</returns>
    Task<IEnumerable<DocumentVersionDto>> GetDocumentVersionsAsync(Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a specific version of a document.
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="version">The version number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document version download information.</returns>
    Task<DocumentDownloadDto> DownloadDocumentVersionAsync(Guid documentId, int version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches documents by name, content, or metadata.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="libraryId">Optional library ID to limit search scope.</param>
    /// <param name="folderId">Optional folder ID to limit search scope.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of matching documents.</returns>
    Task<IEnumerable<DocumentDto>> SearchDocumentsAsync(string searchTerm, Guid? libraryId = null, Guid? folderId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks out a document for editing.
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="checkedOutBy">The user ID who checked out the document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the document was checked out successfully, false otherwise.</returns>
    Task<bool> CheckOutDocumentAsync(Guid documentId, Guid checkedOutBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks in a document after editing.
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="file">The updated file (optional).</param>
    /// <param name="versionComment">The version comment (optional).</param>
    /// <param name="checkedInBy">The user ID who checked in the document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated document information.</returns>
    Task<DocumentDto> CheckInDocumentAsync(Guid documentId, IFormFile? file, string? versionComment, Guid checkedInBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a document checkout.
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="cancelledBy">The user ID who cancelled the checkout.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the checkout was cancelled successfully, false otherwise.</returns>
    Task<bool> CancelCheckOutAsync(Guid documentId, Guid cancelledBy, CancellationToken cancellationToken = default);
}
