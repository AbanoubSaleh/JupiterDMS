using AutoMapper;
using JupiterDMS.Application.Common.DTOs;
using JupiterDMS.Application.Services;
using JupiterDMS.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JupiterDMS.API.Controllers;

/// <summary>
/// Controller for document management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IMapper _mapper;
    private readonly ILogger<DocumentsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentsController"/> class.
    /// </summary>
    /// <param name="documentService">The document service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger.</param>
    public DocumentsController(
        IDocumentService documentService,
        IMapper mapper,
        ILogger<DocumentsController> logger)
    {
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all documents in a folder.
    /// </summary>
    /// <param name="folderId">The folder ID.</param>
    /// <param name="includeDeleted">Whether to include deleted documents.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of documents.</returns>
    /// <response code="200">Documents retrieved successfully.</response>
    /// <response code="403">Forbidden - Viewer access required.</response>
    [HttpGet("folder/{folderId:guid}")]
    [Authorize(Policy = DomainConstants.Auth.ViewerPolicy)]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocumentsByFolderAsync(
        Guid folderId,
        [FromQuery] bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documents = await _documentService.GetDocumentsByFolderAsync(folderId, includeDeleted, cancellationToken);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents for folder {FolderId}", folderId);
            return StatusCode(500, "An error occurred while retrieving documents.");
        }
    }

    /// <summary>
    /// Gets a document by ID.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document information.</returns>
    /// <response code="200">Document retrieved successfully.</response>
    /// <response code="404">Document not found.</response>
    /// <response code="403">Forbidden - Viewer access required.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = DomainConstants.Auth.ViewerPolicy)]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DocumentDto>> GetDocumentByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _documentService.GetDocumentByIdAsync(id, cancellationToken);
            if (document == null)
            {
                return NotFound($"Document with ID '{id}' not found.");
            }

            return Ok(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document {DocumentId}", id);
            return StatusCode(500, "An error occurred while retrieving the document.");
        }
    }

    /// <summary>
    /// Uploads a new document.
    /// </summary>
    /// <param name="request">The document upload request.</param>
    /// <param name="file">The uploaded file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created document information.</returns>
    /// <response code="201">Document uploaded successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="403">Forbidden - Editor access required.</response>
    [HttpPost("upload")]
    [Authorize(Policy = DomainConstants.Auth.EditorPolicy)]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DocumentDto>> UploadDocumentAsync(
        [FromForm] UploadDocumentDto request,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("File is required.");
            }

            var userIdClaim = User.FindFirst(DomainConstants.Jwt.UserIdClaimType);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var document = await _documentService.UploadDocumentAsync(request, file, userId, cancellationToken);

            _logger.LogInformation("Document uploaded successfully: {DocumentName} (ID: {DocumentId}) by user {UserId}", 
                document.Name, document.Id, userId);

            return CreatedAtAction(nameof(GetDocumentByIdAsync), new { id = document.Id }, document);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return StatusCode(500, "An error occurred while uploading the document.");
        }
    }

    /// <summary>
    /// Updates document metadata.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="request">The document update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated document information.</returns>
    /// <response code="200">Document updated successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">Document not found.</response>
    /// <response code="403">Forbidden - Editor access required.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = DomainConstants.Auth.EditorPolicy)]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DocumentDto>> UpdateDocumentAsync(
        Guid id,
        [FromBody] UpdateDocumentDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != request.Id)
            {
                return BadRequest("ID in URL does not match ID in request body.");
            }

            var userIdClaim = User.FindFirst(DomainConstants.Jwt.UserIdClaimType);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var document = await _documentService.UpdateDocumentAsync(request, userId, cancellationToken);

            _logger.LogInformation("Document updated successfully: {DocumentName} (ID: {DocumentId}) by user {UserId}", 
                document.Name, document.Id, userId);

            return Ok(document);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document {DocumentId}", id);
            return StatusCode(500, "An error occurred while updating the document.");
        }
    }

    /// <summary>
    /// Downloads a document.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document file.</returns>
    /// <response code="200">Document downloaded successfully.</response>
    /// <response code="404">Document not found.</response>
    /// <response code="403">Forbidden - Viewer access required.</response>
    [HttpGet("{id:guid}/download")]
    [Authorize(Policy = DomainConstants.Auth.ViewerPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DownloadDocumentAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var download = await _documentService.DownloadDocumentAsync(id, cancellationToken);

            return File(download.FileStream, download.ContentType, download.Name);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {DocumentId}", id);
            return StatusCode(500, "An error occurred while downloading the document.");
        }
    }

    /// <summary>
    /// Deletes a document (soft delete).
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="200">Document deleted successfully.</response>
    /// <response code="404">Document not found.</response>
    /// <response code="403">Forbidden - Editor access required.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = DomainConstants.Auth.EditorPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteDocumentAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(DomainConstants.Jwt.UserIdClaimType);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var success = await _documentService.DeleteDocumentAsync(id, userId, cancellationToken);
            if (!success)
            {
                return NotFound($"Document with ID '{id}' not found.");
            }

            _logger.LogInformation("Document deleted successfully: ID {DocumentId} by user {UserId}", id, userId);
            return Ok("Document deleted successfully.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", id);
            return StatusCode(500, "An error occurred while deleting the document.");
        }
    }

    /// <summary>
    /// Moves a document to a different folder.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="request">The move request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated document information.</returns>
    /// <response code="200">Document moved successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">Document not found.</response>
    /// <response code="403">Forbidden - Editor access required.</response>
    [HttpPost("{id:guid}/move")]
    [Authorize(Policy = DomainConstants.Auth.EditorPolicy)]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DocumentDto>> MoveDocumentAsync(
        Guid id,
        [FromBody] MoveDocumentDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(DomainConstants.Jwt.UserIdClaimType);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var document = await _documentService.MoveDocumentAsync(id, request.TargetFolderId, userId, cancellationToken);

            _logger.LogInformation("Document moved successfully: {DocumentName} (ID: {DocumentId}) to folder {FolderId} by user {UserId}", 
                document.Name, document.Id, request.TargetFolderId, userId);

            return Ok(document);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving document {DocumentId}", id);
            return StatusCode(500, "An error occurred while moving the document.");
        }
    }

    /// <summary>
    /// Creates a new version of a document.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="file">The new file version.</param>
    /// <param name="versionComment">The version comment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated document information.</returns>
    /// <response code="200">Document version created successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">Document not found.</response>
    /// <response code="403">Forbidden - Editor access required.</response>
    [HttpPost("{id:guid}/versions")]
    [Authorize(Policy = DomainConstants.Auth.EditorPolicy)]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DocumentDto>> CreateDocumentVersionAsync(
        Guid id,
        IFormFile file,
        [FromForm] string? versionComment,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is required.");
            }

            var userIdClaim = User.FindFirst(DomainConstants.Jwt.UserIdClaimType);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var document = await _documentService.CreateDocumentVersionAsync(id, file, versionComment, userId, cancellationToken);

            _logger.LogInformation("Document version created successfully: {DocumentName} v{Version} (ID: {DocumentId}) by user {UserId}",
                document.Name, document.CurrentVersion, document.Id, userId);

            return Ok(document);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document version for {DocumentId}", id);
            return StatusCode(500, "An error occurred while creating the document version.");
        }
    }

    /// <summary>
    /// Gets all versions of a document.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of document versions.</returns>
    /// <response code="200">Document versions retrieved successfully.</response>
    /// <response code="403">Forbidden - Viewer access required.</response>
    [HttpGet("{id:guid}/versions")]
    [Authorize(Policy = DomainConstants.Auth.ViewerPolicy)]
    [ProducesResponseType(typeof(IEnumerable<DocumentVersionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<DocumentVersionDto>>> GetDocumentVersionsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var versions = await _documentService.GetDocumentVersionsAsync(id, cancellationToken);
            return Ok(versions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document versions for {DocumentId}", id);
            return StatusCode(500, "An error occurred while retrieving document versions.");
        }
    }

    /// <summary>
    /// Downloads a specific version of a document.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="version">The version number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document version file.</returns>
    /// <response code="200">Document version downloaded successfully.</response>
    /// <response code="404">Document version not found.</response>
    /// <response code="403">Forbidden - Viewer access required.</response>
    [HttpGet("{id:guid}/versions/{version:int}/download")]
    [Authorize(Policy = DomainConstants.Auth.ViewerPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DownloadDocumentVersionAsync(
        Guid id,
        int version,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var download = await _documentService.DownloadDocumentVersionAsync(id, version, cancellationToken);

            return File(download.FileStream, download.ContentType, download.Name);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document version {Version} for {DocumentId}", version, id);
            return StatusCode(500, "An error occurred while downloading the document version.");
        }
    }

    /// <summary>
    /// Searches documents by name, content, or metadata.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="libraryId">Optional library ID to limit search scope.</param>
    /// <param name="folderId">Optional folder ID to limit search scope.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of matching documents.</returns>
    /// <response code="200">Search completed successfully.</response>
    /// <response code="400">Invalid search term.</response>
    /// <response code="403">Forbidden - Viewer access required.</response>
    [HttpGet("search")]
    [Authorize(Policy = DomainConstants.Auth.ViewerPolicy)]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> SearchDocumentsAsync(
        [FromQuery] string searchTerm,
        [FromQuery] Guid? libraryId = null,
        [FromQuery] Guid? folderId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest("Search term is required.");
            }

            var documents = await _documentService.SearchDocumentsAsync(searchTerm, libraryId, folderId, cancellationToken);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching documents with term: {SearchTerm}", searchTerm);
            return StatusCode(500, "An error occurred while searching documents.");
        }
    }

    /// <summary>
    /// Checks out a document for editing.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="200">Document checked out successfully.</response>
    /// <response code="400">Document cannot be checked out.</response>
    /// <response code="404">Document not found.</response>
    /// <response code="403">Forbidden - Editor access required.</response>
    [HttpPost("{id:guid}/checkout")]
    [Authorize(Policy = DomainConstants.Auth.EditorPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CheckOutDocumentAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(DomainConstants.Jwt.UserIdClaimType);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var success = await _documentService.CheckOutDocumentAsync(id, userId, cancellationToken);
            if (!success)
            {
                return NotFound($"Document with ID '{id}' not found.");
            }

            _logger.LogInformation("Document checked out successfully: ID {DocumentId} by user {UserId}", id, userId);
            return Ok("Document checked out successfully.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking out document {DocumentId}", id);
            return StatusCode(500, "An error occurred while checking out the document.");
        }
    }

    /// <summary>
    /// Checks in a document after editing.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="file">The updated file (optional).</param>
    /// <param name="versionComment">The version comment (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated document information.</returns>
    /// <response code="200">Document checked in successfully.</response>
    /// <response code="400">Document cannot be checked in.</response>
    /// <response code="404">Document not found.</response>
    /// <response code="403">Forbidden - Editor access required.</response>
    [HttpPost("{id:guid}/checkin")]
    [Authorize(Policy = DomainConstants.Auth.EditorPolicy)]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DocumentDto>> CheckInDocumentAsync(
        Guid id,
        IFormFile? file,
        [FromForm] string? versionComment,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(DomainConstants.Jwt.UserIdClaimType);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var document = await _documentService.CheckInDocumentAsync(id, file, versionComment, userId, cancellationToken);

            _logger.LogInformation("Document checked in successfully: {DocumentName} (ID: {DocumentId}) by user {UserId}",
                document.Name, document.Id, userId);

            return Ok(document);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in document {DocumentId}", id);
            return StatusCode(500, "An error occurred while checking in the document.");
        }
    }

    /// <summary>
    /// Cancels a document checkout.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="200">Checkout cancelled successfully.</response>
    /// <response code="400">Checkout cannot be cancelled.</response>
    /// <response code="404">Document not found.</response>
    /// <response code="403">Forbidden - Editor access required.</response>
    [HttpPost("{id:guid}/cancel-checkout")]
    [Authorize(Policy = DomainConstants.Auth.EditorPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelCheckOutAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(DomainConstants.Jwt.UserIdClaimType);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var success = await _documentService.CancelCheckOutAsync(id, userId, cancellationToken);
            if (!success)
            {
                return NotFound($"Document with ID '{id}' not found or not checked out.");
            }

            _logger.LogInformation("Document checkout cancelled successfully: ID {DocumentId} by user {UserId}", id, userId);
            return Ok("Document checkout cancelled successfully.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling checkout for document {DocumentId}", id);
            return StatusCode(500, "An error occurred while cancelling the document checkout.");
        }
    }
}
