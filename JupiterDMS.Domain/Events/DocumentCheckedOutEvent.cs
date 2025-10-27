using JupiterDMS.Domain.Common;

namespace JupiterDMS.Domain.Events;

/// <summary>
/// Domain event raised when a document is checked out.
/// </summary>
public class DocumentCheckedOutEvent : IDomainEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentCheckedOutEvent"/> class.
    /// </summary>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="documentName">The document name.</param>
    /// <param name="checkedOutBy">The user who checked out the document.</param>
    /// <param name="checkoutExpiry">The checkout expiry date.</param>
    public DocumentCheckedOutEvent(Guid documentId, string documentName, Guid checkedOutBy, DateTime? checkoutExpiry)
    {
        DocumentId = documentId;
        DocumentName = documentName;
        CheckedOutBy = checkedOutBy;
        CheckoutExpiry = checkoutExpiry;
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
    /// Gets the user who checked out the document.
    /// </summary>
    public Guid CheckedOutBy { get; }

    /// <summary>
    /// Gets the checkout expiry date.
    /// </summary>
    public DateTime? CheckoutExpiry { get; }

    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    public DateTime OccurredOn { get; }
}
