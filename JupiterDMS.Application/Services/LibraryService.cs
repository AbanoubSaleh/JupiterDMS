using JupiterDMS.Application.Common.Interfaces;
using JupiterDMS.Domain.Entities;

namespace JupiterDMS.Application.Services;

/// <summary>
/// Service implementation for library operations.
/// </summary>
public class LibraryService : ILibraryService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    public LibraryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
    public async Task<Library> CreateLibraryAsync(Library library, CancellationToken cancellationToken = default)
    {
        library.Id = Guid.NewGuid();
        library.CreatedOn = DateTime.UtcNow;
        library.CreatedBy = Guid.NewGuid(); // TODO: Get from current user service
        
        await _unitOfWork.Libraries.AddAsync(library, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return library;
    }

    /// <inheritdoc/>
    public async Task<Library> UpdateLibraryAsync(Library library, CancellationToken cancellationToken = default)
    {
        var existingLibrary = await _unitOfWork.Libraries.GetByIdAsync(library.Id, cancellationToken);
        if (existingLibrary == null || existingLibrary.IsDeleted)
        {
            throw new InvalidOperationException("Library not found or has been deleted.");
        }

        existingLibrary.Name = library.Name;
        existingLibrary.Description = library.Description;
        existingLibrary.ModifiedOn = DateTime.UtcNow;
        existingLibrary.ModifiedBy = Guid.NewGuid(); // TODO: Get from current user service

        _unitOfWork.Libraries.Update(existingLibrary);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return existingLibrary;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteLibraryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var library = await _unitOfWork.Libraries.GetByIdAsync(id, cancellationToken);
        if (library == null || library.IsDeleted)
        {
            return false;
        }

        library.IsDeleted = true;
        library.ModifiedOn = DateTime.UtcNow;
        library.ModifiedBy = Guid.NewGuid(); // TODO: Get from current user service

        _unitOfWork.Libraries.Update(library);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
