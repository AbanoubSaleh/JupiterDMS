using JupiterDMS.Domain.Common;
using JupiterDMS.Domain.Enums;

namespace JupiterDMS.Domain.Entities;

/// <summary>
/// Represents a document in the document management system.
/// </summary>
public class Document : BaseEntity
{
    /// <summary>
    /// Gets or sets the name of the document.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder identifier this document belongs to.
    /// </summary>
    public Guid FolderId { get; set; }

    /// <summary>
    /// Gets or sets the physical file path where the document is stored.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current version number of the document.
    /// </summary>
    public int CurrentVersion { get; set; } = 1;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the content type (MIME type) of the document.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file extension.
    /// </summary>
    public string FileExtension { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata as JSON string.
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// Gets or sets the status of the document.
    /// </summary>
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

    /// <summary>
    /// Gets or sets the checksum/hash of the file for integrity verification.
    /// </summary>
    public string? FileHash { get; set; }

    /// <summary>
    /// Gets or sets the file type category.
    /// </summary>
    public FileType FileType { get; set; } = FileType.Unknown;

    /// <summary>
    /// Gets or sets the checkout status of the document.
    /// </summary>
    public CheckoutStatus CheckoutStatus { get; set; } = CheckoutStatus.Available;

    /// <summary>
    /// Gets or sets the user who has checked out the document.
    /// </summary>
    public Guid? CheckedOutBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the document was checked out.
    /// </summary>
    public DateTime? CheckedOutOn { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the checkout expires.
    /// </summary>
    public DateTime? CheckoutExpiry { get; set; }

    /// <summary>
    /// Gets or sets the document title (display name).
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the document description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the document tags as comma-separated values.
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the folder this document belongs to.
    /// </summary>
    public virtual Folder Folder { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user who has checked out the document.
    /// </summary>
    public virtual User? CheckedOutByUser { get; set; }

    /// <summary>
    /// Gets or sets the collection of document versions.
    /// </summary>
    public virtual ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
}

