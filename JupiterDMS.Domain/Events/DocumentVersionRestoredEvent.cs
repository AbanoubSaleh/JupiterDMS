using JupiterDMS.Domain.Common;

namespace JupiterDMS.Domain.Events;

/// <summary>
/// Domain event raised when a document version is restored.
/// </summary>
public class DocumentVersionRestoredEvent : IDomainEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentVersionRestoredEvent"/> class.
    /// </summary>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="documentName">The document name.</param>
    /// <param name="restoredVersionNumber">The version number that was restored.</param>
    /// <param name="newVersionNumber">The new version number created from the restore.</param>
    /// <param name="restoredBy">The user who restored the version.</param>
    public DocumentVersionRestoredEvent(Guid documentId, string documentName, int restoredVersionNumber, int newVersionNumber, Guid restoredBy)
    {
        DocumentId = documentId;
        DocumentName = documentName;
        RestoredVersionNumber = restoredVersionNumber;
        NewVersionNumber = newVersionNumber;
        RestoredBy = restoredBy;
        OccurredOn = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the document identifier.
    /// </summary>
    public Guid DocumentId { get; }

    /// <summary>
    /// Gets the document name.
    /// </summary>
    public string DocumentName { get; }

    /// <summary>
    /// Gets the version number that was restored.
    /// </summary>
    public int RestoredVersionNumber { get; }

    /// <summary>
    /// Gets the new version number created from the restore.
    /// </summary>
    public int NewVersionNumber { get; }

    /// <summary>
    /// Gets the user who restored the version.
    /// </summary>
    public Guid RestoredBy { get; }

    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; }
}
