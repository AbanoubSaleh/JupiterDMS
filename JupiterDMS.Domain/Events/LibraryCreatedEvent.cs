using JupiterDMS.Domain.Common;

namespace JupiterDMS.Domain.Events;

/// <summary>
/// Domain event raised when a library is created.
/// </summary>
public class LibraryCreatedEvent : IDomainEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryCreatedEvent"/> class.
    /// </summary>
    /// <param name="libraryId">The identifier of the created library.</param>
    /// <param name="libraryName">The name of the created library.</param>
    /// <param name="createdBy">The identifier of the user who created the library.</param>
    public LibraryCreatedEvent(Guid libraryId, string libraryName, Guid createdBy)
    {
        LibraryId = libraryId;
        LibraryName = libraryName;
        CreatedBy = createdBy;
        OccurredOn = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the identifier of the created library.
    /// </summary>
    public Guid LibraryId { get; }

    /// <summary>
    /// Gets the name of the created library.
    /// </summary>
    public string LibraryName { get; }

    /// <summary>
    /// Gets the identifier of the user who created the library.
    /// </summary>
    public Guid CreatedBy { get; }

    /// <inheritdoc/>
    public DateTime OccurredOn { get; }
}

