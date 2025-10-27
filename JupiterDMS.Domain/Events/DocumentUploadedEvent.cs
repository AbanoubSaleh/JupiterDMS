using JupiterDMS.Domain.Common;

namespace JupiterDMS.Domain.Events;

/// <summary>
/// Domain event raised when a document is uploaded.
/// </summary>
public class DocumentUploadedEvent : IDomainEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentUploadedEvent"/> class.
    /// </summary>
    /// <param name="documentId">The identifier of the uploaded document.</param>
    /// <param name="documentName">The name of the uploaded document.</param>
    /// <param name="uploadedBy">The identifier of the user who uploaded the document.</param>
    public DocumentUploadedEvent(Guid documentId, string documentName, Guid uploadedBy)
    {
        DocumentId = documentId;
        DocumentName = documentName;
        UploadedBy = uploadedBy;
        OccurredOn = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the identifier of the uploaded document.
    /// </summary>
    public Guid DocumentId { get; }

    /// <summary>
    /// Gets the name of the uploaded document.
    /// </summary>
    public string DocumentName { get; }

    /// <summary>
    /// Gets the identifier of the user who uploaded the document.
    /// </summary>
    public Guid UploadedBy { get; }

    /// <inheritdoc/>
    public DateTime OccurredOn { get; }
}

