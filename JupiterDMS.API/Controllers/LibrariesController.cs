using Microsoft.AspNetCore.Mvc;
using JupiterDMS.Application.Services;
using JupiterDMS.Domain.Entities;

namespace JupiterDMS.API.Controllers;

/// <summary>
/// API controller for managing libraries.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LibrariesController : ControllerBase
{
    private readonly ILibraryService _libraryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibrariesController"/> class.
    /// </summary>
    /// <param name="libraryService">The library service.</param>
    public LibrariesController(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    /// <summary>
    /// Gets all libraries.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive libraries.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of libraries.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Library>>> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var libraries = await _libraryService.GetAllLibrariesAsync(includeInactive, cancellationToken);
        return Ok(libraries);
    }

    /// <summary>
    /// Gets a library by its identifier.
    /// </summary>
    /// <param name="id">The library identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The library if found; otherwise, not found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Library>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var library = await _libraryService.GetLibraryByIdAsync(id, cancellationToken);

        if (library == null)
        {
            return NotFound();
        }

        return Ok(library);
    }

    /// <summary>
    /// Creates a new library.
    /// </summary>
    /// <param name="library">The library to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created library.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Library>> Create(
        Library library,
        CancellationToken cancellationToken = default)
    {
        var createdLibrary = await _libraryService.CreateLibraryAsync(library, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdLibrary.Id }, createdLibrary);
    }

    /// <summary>
    /// Updates an existing library.
    /// </summary>
    /// <param name="id">The library identifier.</param>
    /// <param name="library">The updated library data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated library.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Library>> Update(
        Guid id,
        Library library,
        CancellationToken cancellationToken = default)
    {
        if (id != library.Id)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            var updatedLibrary = await _libraryService.UpdateLibraryAsync(library, cancellationToken);
            return Ok(updatedLibrary);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Deletes a library.
    /// </summary>
    /// <param name="id">The library identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var deleted = await _libraryService.DeleteLibraryAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}

