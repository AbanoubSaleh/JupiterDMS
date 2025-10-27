using JupiterDMS.Application.Common.Interfaces;
using JupiterDMS.Domain.Constants;
using JupiterDMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JupiterDMS.Application.Common.DTOs;
using AutoMapper;

namespace JupiterDMS.API.Controllers;

/// <summary>
/// Controller for folder management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class FoldersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FoldersController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FoldersController"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="fileStorageService">The file storage service.</param>
    /// <param name="logger">The logger.</param>
    public FoldersController(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFileStorageService fileStorageService,
        ILogger<FoldersController> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all folders in a library.
    /// </summary>
    /// <param name="libraryId">The library ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of folders.</returns>
    /// <response code="200">Folders retrieved successfully.</response>
    /// <response code="403">Forbidden - Viewer access required.</response>
    [HttpGet("library/{libraryId:guid}")]
    [Authorize(Policy = DomainConstants.Auth.ViewerPolicy)]
    [ProducesResponseType(typeof(IEnumerable<FolderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<FolderDto>>> GetFoldersByLibraryAsync(
        Guid libraryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var folders = await _unitOfWork.Folders.GetAllAsync(cancellationToken);
            var libraryFolders = folders
                .Where(f => f.LibraryId == libraryId && !f.IsDeleted)
                .OrderBy(f => f.Name);

            var folderDtos = _mapper.Map<IEnumerable<FolderDto>>(libraryFolders);
            return Ok(folderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving folders for library {LibraryId}", libraryId);
            return StatusCode(500, "An error occurred while retrieving folders.");
        }
    }

    /// <summary>
    /// Gets a folder by ID.
    /// </summary>
    /// <param name="id">The folder ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The folder information.</returns>
    /// <response code="200">Folder retrieved successfully.</response>
    /// <response code="404">Folder not found.</response>
    /// <response code="403">Forbidden - Viewer access required.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = DomainConstants.Auth.ViewerPolicy)]
    [ProducesResponseType(typeof(FolderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FolderDto>> GetFolderByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var folder = await _unitOfWork.Folders.GetByIdAsync(id, cancellationToken);
            if (folder == null || folder.IsDeleted)
            {
                return NotFound($"Folder with ID '{id}' not found.");
            }

            var folderDto = _mapper.Map<FolderDto>(folder);
            return Ok(folderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving folder {FolderId}", id);
            return StatusCode(500, "An error occurred while retrieving the folder.");
        }
    }

    /// <summary>
    /// Creates a new folder.
    /// </summary>
    /// <param name="createFolderDto">The folder creation data (excludes auto-generated fields).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created folder.</returns>
    /// <response code="201">Folder created successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="403">Forbidden - Editor access required.</response>
    [HttpPost]
    [Authorize(Policy = DomainConstants.Auth.EditorPolicy)]
    [ProducesResponseType(typeof(FolderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FolderDto>> CreateFolderAsync(
        [FromBody] CreateFolderDto createFolderDto,
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

            // Map DTO to entity
            var folder = _mapper.Map<Folder>(createFolderDto);

            // Verify library exists
            var library = await _unitOfWork.Libraries.GetByIdAsync(folder.LibraryId, cancellationToken);
            if (library == null)
            {
                return BadRequest($"Library with ID '{folder.LibraryId}' not found.");
            }

            // Verify parent folder exists if specified
            if (folder.ParentFolderId.HasValue)
            {
                var parentFolder = await _unitOfWork.Folders.GetByIdAsync(folder.ParentFolderId.Value, cancellationToken);
                if (parentFolder == null || parentFolder.IsDeleted)
                {
                    return BadRequest($"Parent folder with ID '{folder.ParentFolderId}' not found.");
                }

                // Build path based on parent folder
                folder.Path = string.IsNullOrEmpty(parentFolder.Path)
                    ? $"/{folder.Name}"
                    : $"{parentFolder.Path}/{folder.Name}";
            }
            else
            {
                folder.Path = $"/{folder.Name}";
            }

            // Set auto-generated fields
            folder.Id = Guid.NewGuid();
            folder.CreatedOn = DateTime.UtcNow;
            folder.CreatedBy = userId;
            folder.IsDeleted = false;

            // Create physical directory for the folder
            try
            {
                await _fileStorageService.CreateFolderDirectoryAsync(library.Name, folder.Path.TrimStart('/'), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating physical directory for folder: {FolderPath}", folder.Path);
                return StatusCode(500, "An error occurred while creating the folder directory.");
            }

            await _unitOfWork.Folders.AddAsync(folder, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Folder created successfully: {FolderName} (ID: {FolderId}) by user {UserId}",
                folder.Name, folder.Id, userId);

            var folderDto = _mapper.Map<FolderDto>(folder);
            return StatusCode(StatusCodes.Status201Created, folderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating folder");
            return StatusCode(500, "An error occurred while creating the folder.");
        }
    }

    /// <summary>
    /// Updates a folder.
    /// </summary>
    /// <param name="id">The folder ID.</param>
    /// <param name="folder">The updated folder information.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated folder.</returns>
    /// <response code="200">Folder updated successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">Folder not found.</response>
    /// <response code="403">Forbidden - Editor access required.</response>
    [HttpPut]
    [Authorize(Policy = DomainConstants.Auth.EditorPolicy)]
    [ProducesResponseType(typeof(FolderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FolderDto>> UpdateFolderAsync(
        [FromBody] UpdateFolderDto updateFolderDto,
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

            var existingFolder = await _unitOfWork.Folders.GetByIdAsync(updateFolderDto.Id, cancellationToken);
            if (existingFolder == null || existingFolder.IsDeleted)
            {
                return NotFound($"Folder with ID '{updateFolderDto.Id}' not found.");
            }

            // Update properties from DTO
            existingFolder.Name = updateFolderDto.Name;
            existingFolder.Description = updateFolderDto.Description;
            existingFolder.ModifiedOn = DateTime.UtcNow;
            existingFolder.ModifiedBy = userId;

            // Update path if name changed
            if (existingFolder.ParentFolderId.HasValue)
            {
                var parentFolder = await _unitOfWork.Folders.GetByIdAsync(existingFolder.ParentFolderId.Value, cancellationToken);
                existingFolder.Path = string.IsNullOrEmpty(parentFolder?.Path) 
                    ? $"/{existingFolder.Name}" 
                    : $"{parentFolder.Path}/{existingFolder.Name}";
            }
            else
            {
                existingFolder.Path = $"/{existingFolder.Name}";
            }

            _unitOfWork.Folders.Update(existingFolder);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Folder updated successfully: {FolderName} (ID: {FolderId}) by user {UserId}", 
                existingFolder.Name, existingFolder.Id, userId);

            var folderDto = _mapper.Map<FolderDto>(existingFolder);
            return Ok(folderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating folder {FolderId}", updateFolderDto.Id);
            return StatusCode(500, "An error occurred while updating the folder.");
        }
    }

    /// <summary>
    /// Deletes a folder (soft delete).
    /// </summary>
    /// <param name="id">The folder ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="200">Folder deleted successfully.</response>
    /// <response code="404">Folder not found.</response>
    /// <response code="403">Forbidden - Editor access required.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = DomainConstants.Auth.EditorPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteFolderAsync(
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

            var folder = await _unitOfWork.Folders.GetByIdAsync(id, cancellationToken);
            if (folder == null || folder.IsDeleted)
            {
                return NotFound($"Folder with ID '{id}' not found.");
            }

            // Check if folder has documents
            var documents = await _unitOfWork.Documents.GetAllAsync(cancellationToken);
            var hasDocuments = documents.Any(d => d.FolderId == id && !d.IsDeleted);
            if (hasDocuments)
            {
                return BadRequest("Cannot delete folder that contains documents.");
            }

            // Check if folder has subfolders
            var folders = await _unitOfWork.Folders.GetAllAsync(cancellationToken);
            var hasSubfolders = folders.Any(f => f.ParentFolderId == id && !f.IsDeleted);
            if (hasSubfolders)
            {
                return BadRequest("Cannot delete folder that contains subfolders.");
            }

            // Soft delete
            folder.IsDeleted = true;
            folder.ModifiedOn = DateTime.UtcNow;
            folder.ModifiedBy = userId;

            _unitOfWork.Folders.Update(folder);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Folder deleted successfully: {FolderName} (ID: {FolderId}) by user {UserId}", 
                folder.Name, folder.Id, userId);

            return Ok("Folder deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting folder {FolderId}", id);
            return StatusCode(500, "An error occurred while deleting the folder.");
        }
    }
}
