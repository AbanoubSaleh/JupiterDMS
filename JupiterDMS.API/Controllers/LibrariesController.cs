using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using JupiterDMS.Application.Services;
using JupiterDMS.Domain.Entities;
using JupiterDMS.Domain.Constants;
using JupiterDMS.Application.Common.DTOs;
using JupiterDMS.Application.Common.Interfaces;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<LibrariesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibrariesController"/> class.
    /// </summary>
    /// <param name="libraryService">The library service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger.</param>
    public LibrariesController(ILibraryService libraryService, IUnitOfWork unitOfWork, IMapper mapper, ILogger<LibrariesController> logger)
    {
        _libraryService = libraryService ?? throw new ArgumentNullException(nameof(libraryService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
    [ProducesResponseType(typeof(IEnumerable<LibraryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<LibraryDto>>> GetAllLibrariesAsync(
        [FromQuery] bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var libraries = await _libraryService.GetAllLibrariesAsync(includeDeleted, cancellationToken);
            var libraryDtos = _mapper.Map<IEnumerable<LibraryDto>>(libraries);
            return Ok(libraryDtos);
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
    [ProducesResponseType(typeof(LibraryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LibraryDto>> GetLibraryByIdAsync(
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

            var libraryDto = _mapper.Map<LibraryDto>(library);
            return Ok(libraryDto);
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
    [ProducesResponseType(typeof(LibraryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LibraryDto>> CreateLibraryAsync(
        [FromBody] CreateLibraryDto createLibraryDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var emailClaim = User.FindFirst(ClaimTypes.Email);
            if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value))
            {
                return Unauthorized();
            }

            // Map DTO to entity (auto-generated fields will be set in the service)
            var library = _mapper.Map<Library>(createLibraryDto);
            var createdLibrary = await _libraryService.CreateLibraryAsync(library, emailClaim.Value, cancellationToken);

            _logger.LogInformation("Library created successfully: {LibraryName} (ID: {LibraryId}) by user {UserEmail}",
                createdLibrary.Name, createdLibrary.Id, emailClaim.Value);

            var libraryDto = _mapper.Map<LibraryDto>(createdLibrary);
            return StatusCode(StatusCodes.Status201Created, libraryDto);
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
    [ProducesResponseType(typeof(LibraryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LibraryDto>> UpdateLibraryAsync(
        [FromBody] UpdateLibraryDto updateLibraryDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var emailClaim = User.FindFirst(ClaimTypes.Email);
            if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value))
            {
                return Unauthorized();
            }

            // Map DTO to entity (auto-generated fields will be set in the service)
            var library = _mapper.Map<Library>(updateLibraryDto);
            var updatedLibrary = await _libraryService.UpdateLibraryAsync(library, emailClaim.Value, cancellationToken);

            _logger.LogInformation("Library updated successfully: {LibraryName} (ID: {LibraryId}) by user {UserEmail}",
                updatedLibrary.Name, updatedLibrary.Id, emailClaim.Value);

            var libraryDto = _mapper.Map<LibraryDto>(updatedLibrary);
            return Ok(libraryDto);
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
            var emailClaim = User.FindFirst(ClaimTypes.Email);
            if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value))
            {
                return Unauthorized();
            }

            var success = await _libraryService.DeleteLibraryAsync(id, emailClaim.Value, cancellationToken);
            if (!success)
            {
                return NotFound($"Library with ID '{id}' not found.");
            }

            _logger.LogInformation("Library deleted successfully: ID {LibraryId} by user {UserEmail}", id, emailClaim.Value);
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
    [ProducesResponseType(typeof(LibraryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LibraryDto>> GetLibraryByNameAsync(
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

            var libraryDto = _mapper.Map<LibraryDto>(library);
            return Ok(libraryDto);
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
            var allFolders = await _unitOfWork.Folders.GetAllAsync(cancellationToken);

            var libraryTree = libraries.Select(lib => new
            {
                id = lib.Id,
                name = lib.Name,
                description = lib.Description,
                type = "library",
                children = BuildFolderHierarchy(lib.Id, allFolders)
            });

            return Ok(libraryTree);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving library tree");
            return StatusCode(500, "An error occurred while retrieving the library tree.");
        }
    }

    /// <summary>
    /// Builds the folder hierarchy for a library.
    /// </summary>
    /// <param name="libraryId">The library ID.</param>
    /// <param name="allFolders">All folders in the system.</param>
    /// <returns>The hierarchical folder structure.</returns>
    private object[] BuildFolderHierarchy(Guid libraryId, IEnumerable<Folder> allFolders)
    {
        var libraryFolders = allFolders
            .Where(f => f.LibraryId == libraryId && !f.IsDeleted)
            .ToList();

        // Get root folders (folders without parent)
        var rootFolders = libraryFolders
            .Where(f => f.ParentFolderId == null)
            .OrderBy(f => f.Name)
            .ToList();

        return rootFolders.Select(folder => BuildFolderNode(folder, libraryFolders)).ToArray();
    }

    /// <summary>
    /// Builds a folder node with its children.
    /// </summary>
    /// <param name="folder">The folder.</param>
    /// <param name="allFolders">All folders in the library.</param>
    /// <returns>The folder node.</returns>
    private object BuildFolderNode(Folder folder, List<Folder> allFolders)
    {
        var children = allFolders
            .Where(f => f.ParentFolderId == folder.Id)
            .OrderBy(f => f.Name)
            .Select(childFolder => BuildFolderNode(childFolder, allFolders))
            .ToArray();

        return new
        {
            id = folder.Id,
            name = folder.Name,
            description = folder.Description,
            type = "folder",
            path = folder.Path,
            parentFolderId = folder.ParentFolderId,
            children = children
        };
    }
}

