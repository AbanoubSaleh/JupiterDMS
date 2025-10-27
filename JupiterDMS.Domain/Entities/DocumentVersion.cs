using JupiterDMS.Domain.Common;

namespace JupiterDMS.Domain.Entities;

/// <summary>
/// Represents a version of a document in the document management system.
/// </summary>
public class DocumentVersion : BaseEntity
{
    /// <summary>
    /// Gets or sets the document identifier this version belongs to.
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the version number.
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Gets or sets the physical file path where this version is stored.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes for this version.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the checksum/hash of the file for this version.
    /// </summary>
    public string? FileHash { get; set; }

    /// <summary>
    /// Gets or sets the version comment or description.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the document this version belongs to.
    /// </summary>
    public virtual Document Document { get; set; } = null!;
}

