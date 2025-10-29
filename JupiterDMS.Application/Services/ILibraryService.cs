using JupiterDMS.Domain.Entities;

namespace JupiterDMS.Application.Services;

/// <summary>
/// Service interface for library operations.
/// </summary>
public interface ILibraryService
{
    /// <summary>
    /// Gets all libraries.
    /// </summary>
    /// <param name="includeDeleted">Whether to include deleted libraries.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of libraries.</returns>
    Task<IEnumerable<Library>> GetAllLibrariesAsync(bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a library by ID.
    /// </summary>
    /// <param name="id">The library ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The library if found; otherwise, null.</returns>
    Task<Library?> GetLibraryByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a library by name.
    /// </summary>
    /// <param name="name">The library name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The library if found; otherwise, null.</returns>
    Task<Library?> GetLibraryByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new library.
    /// </summary>
    /// <param name="library">The library to create.</param>
    /// <param name="createdBy">The user ID who is creating the library.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created library.</returns>
    Task<Library> CreateLibraryAsync(Library library, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing library.
    /// </summary>
    /// <param name="library">The library to update.</param>
    /// <param name="updatedBy">The user ID who is updating the library.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated library.</returns>
    Task<Library> UpdateLibraryAsync(Library library, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a library (soft delete).
    /// </summary>
    /// <param name="id">The library ID.</param>
    /// <param name="deletedBy">The user ID who is deleting the library.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted successfully; otherwise, false.</returns>
    Task<bool> DeleteLibraryAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);
}
