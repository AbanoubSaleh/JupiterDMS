using JupiterDMS.Application.Common.Interfaces;
using JupiterDMS.Domain.Entities;

namespace JupiterDMS.Application.Services;

/// <summary>
/// Service implementation for library operations.
/// </summary>
public class LibraryService : ILibraryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="fileStorageService">The file storage service.</param>
    public LibraryService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Library>> GetAllLibrariesAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var libraries = await _unitOfWork.Libraries.GetAllAsync(cancellationToken);
        return includeDeleted ? libraries : libraries.Where(l => !l.IsDeleted);
    }

    /// <inheritdoc/>
    public async Task<Library?> GetLibraryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var library = await _unitOfWork.Libraries.GetByIdAsync(id, cancellationToken);
        return library?.IsDeleted == true ? null : library;
    }

    /// <inheritdoc/>
    public async Task<Library?> GetLibraryByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var libraries = await _unitOfWork.Libraries.GetAllAsync(cancellationToken);
        var library = libraries.FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !l.IsDeleted);
        return library;
    }

    /// <inheritdoc/>
    public async Task<Library> CreateLibraryAsync(Library library, string createdBy, CancellationToken cancellationToken = default)
    {
        // Check if library name already exists
        var existingLibrary = await GetLibraryByNameAsync(library.Name, cancellationToken);
        if (existingLibrary != null)
        {
            throw new InvalidOperationException($"A library with the name '{library.Name}' already exists.");
        }

        library.Id = Guid.NewGuid();
        library.CreatedOn = DateTime.UtcNow;
        library.CreatedBy = createdBy;
        library.IsDeleted = false;

        // Create physical directory for the library
        await _fileStorageService.CreateLibraryDirectoryAsync(library.Name, cancellationToken);

        await _unitOfWork.Libraries.AddAsync(library, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return library;
    }

    /// <inheritdoc/>
    public async Task<Library> UpdateLibraryAsync(Library library, string updatedBy, CancellationToken cancellationToken = default)
    {
        var existingLibrary = await _unitOfWork.Libraries.GetByIdAsync(library.Id, cancellationToken);
        if (existingLibrary == null || existingLibrary.IsDeleted)
        {
            throw new InvalidOperationException($"Library with ID '{library.Id}' not found.");
        }

        // Check if new name conflicts with another library
        if (!existingLibrary.Name.Equals(library.Name, StringComparison.OrdinalIgnoreCase))
        {
            var nameConflict = await GetLibraryByNameAsync(library.Name, cancellationToken);
            if (nameConflict != null && nameConflict.Id != library.Id)
            {
                throw new InvalidOperationException($"A library with the name '{library.Name}' already exists.");
            }
        }

        existingLibrary.Name = library.Name;
        existingLibrary.Description = library.Description;
        existingLibrary.IsActive = library.IsActive;
        existingLibrary.ModifiedOn = DateTime.UtcNow;
        existingLibrary.ModifiedBy = updatedBy;

        _unitOfWork.Libraries.Update(existingLibrary);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return existingLibrary;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteLibraryAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var library = await _unitOfWork.Libraries.GetByIdAsync(id, cancellationToken);
        if (library == null || library.IsDeleted)
        {
            return false;
        }

        // Check if library has folders
        var folders = await _unitOfWork.Folders.GetAllAsync(cancellationToken);
        var hasFolders = folders.Any(f => f.LibraryId == id && !f.IsDeleted);
        if (hasFolders)
        {
            throw new InvalidOperationException("Cannot delete library that contains folders.");
        }

        library.IsDeleted = true;
        library.ModifiedOn = DateTime.UtcNow;
        library.ModifiedBy = deletedBy;

        _unitOfWork.Libraries.Update(library);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
