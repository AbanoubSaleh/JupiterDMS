using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JupiterDMS.Application.Services;
using JupiterDMS.Domain.Constants;

namespace JupiterDMS.API.Controllers;

/// <summary>
/// Controller for search operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<SearchController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchController"/> class.
    /// </summary>
    /// <param name="documentService">The document service.</param>
    /// <param name="logger">The logger.</param>
    public SearchController(
        IDocumentService documentService,
        ILogger<SearchController> logger)
    {
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Searches for documents by filename or content.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="type">The search type (filename or fulltext).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of matching documents.</returns>
    /// <response code="200">Search completed successfully.</response>
    /// <response code="400">Invalid search parameters.</response>
    /// <response code="403">Forbidden - Viewer access required.</response>
    [HttpGet("documents")]
    [Authorize(Policy = DomainConstants.Auth.ViewerPolicy)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> SearchDocumentsAsync(
        [FromQuery] string query,
        [FromQuery] string type = "filename",
        [FromQuery] Guid? libraryId = null,
        [FromQuery] Guid? folderId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty.");
            }

            if (query.Length < 2)
            {
                return BadRequest("Search query must be at least 2 characters long.");
            }

            var documents = await _documentService.SearchDocumentsAsync(query, libraryId, folderId, cancellationToken);

            // The DocumentService already performs the search, so we just need to format the results
            var results = documents.Select(d => new
            {
                id = d.Id,
                name = d.Name,
                description = d.Description,
                contentType = d.ContentType,
                fileSizeBytes = d.FileSizeBytes,
                version = d.CurrentVersion,
                createdOn = d.CreatedOn,
                modifiedOn = d.ModifiedOn,
                libraryName = d.LibraryName,
                folderId = d.FolderId,
                matchType = type
            });

            var resultList = results.ToList();
            
            _logger.LogInformation("Search completed: query='{Query}', type='{Type}', results={Count}", 
                query, type, resultList.Count);

            return Ok(new
            {
                query = query,
                type = type,
                count = resultList.Count,
                results = resultList
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during document search: query='{Query}', type='{Type}'", query, type);
            return StatusCode(500, "An error occurred while searching documents.");
        }
    }
}
