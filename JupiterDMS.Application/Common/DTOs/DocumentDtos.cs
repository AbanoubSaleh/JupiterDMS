using System.ComponentModel.DataAnnotations;
using JupiterDMS.Domain.Enums;

namespace JupiterDMS.Application.Common.DTOs;

/// <summary>
/// Data transfer object for document information.
/// </summary>
public class DocumentDto
{
    /// <summary>
    /// Gets or sets the document ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the document name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder ID.
    /// </summary>
    public Guid FolderId { get; set; }

    /// <summary>
    /// Gets or sets the folder name.
    /// </summary>
    public string FolderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder path.
    /// </summary>
    public string FolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the library name.
    /// </summary>
    public string LibraryName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current version.
    /// </summary>
    public int CurrentVersion { get; set; }

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file extension.
    /// </summary>
    public string FileExtension { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the document description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the document tags.
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the document status.
    /// </summary>
    public DocumentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the checkout status.
    /// </summary>
    public CheckoutStatus CheckoutStatus { get; set; }

    /// <summary>
    /// Gets or sets the user who checked out the document.
    /// </summary>
    public string? CheckedOutBy { get; set; }

    /// <summary>
    /// Gets or sets the checkout date.
    /// </summary>
    public DateTime? CheckedOutOn { get; set; }

    /// <summary>
    /// Gets or sets the checkout expiry date.
    /// </summary>
    public DateTime? CheckoutExpiry { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the user who created the document.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last modification date.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified the document.
    /// </summary>
    public string? ModifiedBy { get; set; }
}

/// <summary>
/// Data transfer object for uploading a document.
/// </summary>
public class UploadDocumentDto
{
    /// <summary>
    /// Gets or sets the document name.
    /// </summary>
    [Required(ErrorMessage = "Document name is required.")]
    [StringLength(255, ErrorMessage = "Document name cannot exceed 255 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder ID.
    /// </summary>
    [Required(ErrorMessage = "Folder ID is required.")]
    public Guid FolderId { get; set; }

    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    [StringLength(500, ErrorMessage = "Title cannot exceed 500 characters.")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the document description.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the document tags.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Tags cannot exceed 1000 characters.")]
    public string? Tags { get; set; }
}

/// <summary>
/// Data transfer object for updating a document.
/// </summary>
public class UpdateDocumentDto
{
    /// <summary>
    /// Gets or sets the document ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the document name.
    /// </summary>
    [Required(ErrorMessage = "Document name is required.")]
    [StringLength(255, ErrorMessage = "Document name cannot exceed 255 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    [StringLength(500, ErrorMessage = "Title cannot exceed 500 characters.")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the document description.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the document tags.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Tags cannot exceed 1000 characters.")]
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the document status.
    /// </summary>
    public DocumentStatus Status { get; set; }
}

/// <summary>
/// Data transfer object for document download.
/// </summary>
public class DocumentDownloadDto
{
    /// <summary>
    /// Gets or sets the document name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the file stream.
    /// </summary>
    public Stream FileStream { get; set; } = null!;
}

/// <summary>
/// Data transfer object for document version information.
/// </summary>
public class DocumentVersionDto
{
    /// <summary>
    /// Gets or sets the version ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the document ID.
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the version number.
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Gets or sets the file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the version comment.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the user who created the version.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for moving a document.
/// </summary>
public class MoveDocumentDto
{
    /// <summary>
    /// Gets or sets the target folder ID.
    /// </summary>
    [Required(ErrorMessage = "Target folder ID is required.")]
    public Guid TargetFolderId { get; set; }
}
