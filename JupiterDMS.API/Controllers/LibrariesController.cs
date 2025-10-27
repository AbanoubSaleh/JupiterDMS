using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JupiterDMS.Application.Services;
using JupiterDMS.Domain.Entities;
using JupiterDMS.Domain.Constants;
using JupiterDMS.Application.Common.DTOs;
using AutoMapper;

namespace JupiterDMS.API.Controllers;

/// <summary>
/// API controller for managing libraries.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class LibrariesController : ControllerBase
{
    private readonly ILibraryService _libraryService;
    private readonly IMapper _mapper;
    private readonly ILogger<LibrariesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibrariesController"/> class.
    /// </summary>
    /// <param name="libraryService">The library service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger.</param>
    public LibrariesController(ILibraryService libraryService, IMapper mapper, ILogger<LibrariesController> logger)
    {
        _libraryService = libraryService ?? throw new ArgumentNullException(nameof(libraryService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all libraries.
    /// </summary>
    /// <param name="includeDeleted">Whether to include deleted libraries.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of libraries.</returns>
    /// <response code="200">Libraries retrieved successfully.</response>
    /// <response code="403">Forbidden - Viewer access required.</response>
    [HttpGet]
    [Authorize(Policy = DomainConstants.Auth.ViewerPolicy)]
    [ProducesResponseType(typeof(IEnumerable<Library>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<Library>>> GetAllLibrariesAsync(
        [FromQuery] bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var libraries = await _libraryService.GetAllLibrariesAsync(includeDeleted, cancellationToken);
            return Ok(libraries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving libraries");
            return StatusCode(500, "An error occurred while retrieving libraries.");
        }
    }

    /// <summary>
    /// Gets a library by ID.
    /// </summary>
    /// <param name="id">The library ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The library information.</returns>
    /// <response code="200">Library retrieved successfully.</response>
    /// <response code="404">Library not found.</response>
    /// <response code="403">Forbidden - Viewer access required.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = DomainConstants.Auth.ViewerPolicy)]
    [ProducesResponseType(typeof(Library), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Library>> GetLibraryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var library = await _libraryService.GetLibraryByIdAsync(id, cancellationToken);
            if (library == null)
            {
                return NotFound($"Library with ID '{id}' not found.");
            }

            return Ok(library);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving library {LibraryId}", id);
            return StatusCode(500, "An error occurred while retrieving the library.");
        }
    }

    /// <summary>
    /// Creates a new library.
    /// </summary>
    /// <param name="createLibraryDto">The library creation data (excludes auto-generated fields).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created library.</returns>
    /// <response code="201">Library created successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="403">Forbidden - Admin access required.</response>
    [HttpPost]
    [Authorize(Policy = DomainConstants.Auth.AdminPolicy)]
    [ProducesResponseType(typeof(Library), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Library>> CreateLibraryAsync(
        [FromBody] CreateLibraryDto createLibraryDto,
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

            // Map DTO to entity (auto-generated fields will be set in the service)
            var library = _mapper.Map<Library>(createLibraryDto);
            var createdLibrary = await _libraryService.CreateLibraryAsync(library, userId, cancellationToken);

            _logger.LogInformation("Library created successfully: {LibraryName} (ID: {LibraryId}) by user {UserId}",
                createdLibrary.Name, createdLibrary.Id, userId);

            return StatusCode(StatusCodes.Status201Created, createdLibrary);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating library");
            return StatusCode(500, "An error occurred while creating the library.");
        }
    }

    /// <summary>
    /// Updates a library.
    /// </summary>
    /// <param name="id">The library ID.</param>
    /// <param name="library">The updated library information.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated library.</returns>
    /// <response code="200">Library updated successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">Library not found.</response>
    /// <response code="403">Forbidden - Admin access required.</response>
    [HttpPut]
    [Authorize(Policy = DomainConstants.Auth.AdminPolicy)]
    [ProducesResponseType(typeof(Library), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Library>> UpdateLibraryAsync(
        [FromBody] UpdateLibraryDto updateLibraryDto,
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

            // Map DTO to entity (auto-generated fields will be set in the service)
            var library = _mapper.Map<Library>(updateLibraryDto);
            var updatedLibrary = await _libraryService.UpdateLibraryAsync(library, userId, cancellationToken);

            _logger.LogInformation("Library updated successfully: {LibraryName} (ID: {LibraryId}) by user {UserId}",
                updatedLibrary.Name, updatedLibrary.Id, userId);

            return Ok(updatedLibrary);
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
            _logger.LogError(ex, "Error updating library {LibraryId}", updateLibraryDto.Id);
            return StatusCode(500, "An error occurred while updating the library.");
        }
    }

    /// <summary>
    /// Deletes a library (soft delete).
    /// </summary>
    /// <param name="id">The library ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="200">Library deleted successfully.</response>
    /// <response code="404">Library not found.</response>
    /// <response code="403">Forbidden - Admin access required.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = DomainConstants.Auth.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteLibraryAsync(
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

            var success = await _libraryService.DeleteLibraryAsync(id, userId, cancellationToken);
            if (!success)
            {
                return NotFound($"Library with ID '{id}' not found.");
            }

            _logger.LogInformation("Library deleted successfully: ID {LibraryId} by user {UserId}", id, userId);
            return Ok("Library deleted successfully.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting library {LibraryId}", id);
            return StatusCode(500, "An error occurred while deleting the library.");
        }
    }

    /// <summary>
    /// Gets a library by name.
    /// </summary>
    /// <param name="name">The library name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The library information.</returns>
    /// <response code="200">Library retrieved successfully.</response>
    /// <response code="404">Library not found.</response>
    /// <response code="403">Forbidden - Viewer access required.</response>
    [HttpGet("by-name/{name}")]
    [Authorize(Policy = DomainConstants.Auth.ViewerPolicy)]
    [ProducesResponseType(typeof(Library), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Library>> GetLibraryByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var library = await _libraryService.GetLibraryByNameAsync(name, cancellationToken);
            if (library == null)
            {
                return NotFound($"Library with name '{name}' not found.");
            }

            return Ok(library);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving library by name {LibraryName}", name);
            return StatusCode(500, "An error occurred while retrieving the library.");
        }
    }

    /// <summary>
    /// Gets the library tree structure with folders.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The hierarchical library tree.</returns>
    /// <response code="200">Library tree retrieved successfully.</response>
    /// <response code="403">Forbidden - Viewer access required.</response>
    [HttpGet("tree")]
    [Authorize(Policy = DomainConstants.Auth.ViewerPolicy)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetLibraryTreeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var libraries = await _libraryService.GetAllLibrariesAsync(false, cancellationToken);

            // For now, return a simple structure. This can be enhanced later with folder hierarchy
            var libraryTree = libraries.Select(lib => new
            {
                id = lib.Id,
                name = lib.Name,
                description = lib.Description,
                type = "library",
                children = new object[] { } // Placeholder for folders
            });

            return Ok(libraryTree);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving library tree");
            return StatusCode(500, "An error occurred while retrieving the library tree.");
        }
    }
}

