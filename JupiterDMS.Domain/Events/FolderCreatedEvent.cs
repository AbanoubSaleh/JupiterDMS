using JupiterDMS.Domain.Common;

namespace JupiterDMS.Domain.Events;

/// <summary>
/// Domain event raised when a folder is created.
/// </summary>
public class FolderCreatedEvent : IDomainEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FolderCreatedEvent"/> class.
    /// </summary>
    /// <param name="folderId">The folder identifier.</param>
    /// <param name="folderName">The folder name.</param>
    /// <param name="libraryId">The library identifier.</param>
    /// <param name="parentFolderId">The parent folder identifier.</param>
    /// <param name="createdBy">The user who created the folder.</param>
    public FolderCreatedEvent(Guid folderId, string folderName, Guid libraryId, Guid? parentFolderId, Guid createdBy)
    {
        FolderId = folderId;
        FolderName = folderName;
        LibraryId = libraryId;
        ParentFolderId = parentFolderId;
        CreatedBy = createdBy;
        OccurredOn = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the folder identifier.
    /// </summary>
    public Guid FolderId { get; }

    /// <summary>
    /// Gets the folder name.
    /// </summary>
    public string FolderName { get; }

    /// <summary>
    /// Gets the library identifier.
    /// </summary>
    public Guid LibraryId { get; }

    /// <summary>
    /// Gets the parent folder identifier.
    /// </summary>
    public Guid? ParentFolderId { get; }

    /// <summary>
    /// Gets the user who created the folder.
    /// </summary>
    public Guid CreatedBy { get; }

    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; }
}
