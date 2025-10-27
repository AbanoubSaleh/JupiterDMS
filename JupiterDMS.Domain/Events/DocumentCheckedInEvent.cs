using JupiterDMS.Domain.Common;

namespace JupiterDMS.Domain.Events;

/// <summary>
/// Domain event raised when a document is checked in.
/// </summary>
public class DocumentCheckedInEvent : IDomainEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentCheckedInEvent"/> class.
    /// </summary>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="documentName">The document name.</param>
    /// <param name="checkedInBy">The user who checked in the document.</param>
    /// <param name="newVersionCreated">Whether a new version was created.</param>
    /// <param name="versionNumber">The new version number if created.</param>
    public DocumentCheckedInEvent(Guid documentId, string documentName, Guid checkedInBy, bool newVersionCreated, int? versionNumber)
    {
        DocumentId = documentId;
        DocumentName = documentName;
        CheckedInBy = checkedInBy;
        NewVersionCreated = newVersionCreated;
        VersionNumber = versionNumber;
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
    /// Gets the user who checked in the document.
    /// </summary>
    public Guid CheckedInBy { get; }

    /// <summary>
    /// Gets whether a new version was created.
    /// </summary>
    public bool NewVersionCreated { get; }

    /// <summary>
    /// Gets the new version number if created.
    /// </summary>
    public int? VersionNumber { get; }

    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; }
}
