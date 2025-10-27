using System.ComponentModel.DataAnnotations;

namespace JupiterDMS.WebUI.Models;

/// <summary>
/// View model for document display.
/// </summary>
public class DocumentViewModel
{
    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the document name.
    /// </summary>
    [Display(Name = "Document Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    [Display(Name = "Title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the document description.
    /// </summary>
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the document tags.
    /// </summary>
    [Display(Name = "Tags")]
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the file type.
    /// </summary>
    [Display(Name = "File Type")]
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the formatted file size.
    /// </summary>
    [Display(Name = "Size")]
    public string FormattedSize => FormatFileSize(Size);

    /// <summary>
    /// Gets or sets the current version number.
    /// </summary>
    [Display(Name = "Version")]
    public int CurrentVersion { get; set; }

    /// <summary>
    /// Gets or sets the checkout status.
    /// </summary>
    [Display(Name = "Status")]
    public string CheckoutStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets who checked out the document.
    /// </summary>
    [Display(Name = "Checked Out By")]
    public string? CheckedOutByUsername { get; set; }

    /// <summary>
    /// Gets or sets when the document was checked out.
    /// </summary>
    [Display(Name = "Checked Out On")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
    public DateTime? CheckedOutOn { get; set; }

    /// <summary>
    /// Gets or sets when the checkout expires.
    /// </summary>
    [Display(Name = "Checkout Expires")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
    public DateTime? CheckoutExpiry { get; set; }

    /// <summary>
    /// Gets or sets the folder identifier.
    /// </summary>
    public Guid FolderId { get; set; }

    /// <summary>
    /// Gets or sets the folder name.
    /// </summary>
    [Display(Name = "Folder")]
    public string FolderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder path.
    /// </summary>
    [Display(Name = "Path")]
    public string FolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the library identifier.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the library name.
    /// </summary>
    [Display(Name = "Library")]
    public string LibraryName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the document was created.
    /// </summary>
    [Display(Name = "Created On")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets who created the document.
    /// </summary>
    [Display(Name = "Created By")]
    public string? CreatedByUsername { get; set; }

    /// <summary>
    /// Gets or sets when the document was last modified.
    /// </summary>
    [Display(Name = "Modified On")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets who last modified the document.
    /// </summary>
    [Display(Name = "Modified By")]
    public string? ModifiedByUsername { get; set; }

    /// <summary>
    /// Gets or sets whether the document is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets whether the document is checked out.
    /// </summary>
    public bool IsCheckedOut => CheckoutStatus == "CheckedOut";

    /// <summary>
    /// Gets whether the checkout has expired.
    /// </summary>
    public bool IsCheckoutExpired => CheckoutExpiry.HasValue && CheckoutExpiry.Value < DateTime.UtcNow;

    /// <summary>
    /// Formats file size for display.
    /// </summary>
    /// <param name="bytes">The size in bytes.</param>
    /// <returns>The formatted size string.</returns>
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

/// <summary>
/// View model for uploading a document.
/// </summary>
public class UploadDocumentViewModel
{
    /// <summary>
    /// Gets or sets the file to upload.
    /// </summary>
    [Required(ErrorMessage = "Please select a file to upload")]
    [Display(Name = "File")]
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    [StringLength(500, ErrorMessage = "Title cannot exceed 500 characters")]
    [Display(Name = "Title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the document description.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the document tags.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Tags cannot exceed 1000 characters")]
    [Display(Name = "Tags")]
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the folder identifier.
    /// </summary>
    [Required(ErrorMessage = "Folder is required")]
    public Guid FolderId { get; set; }

    /// <summary>
    /// Gets or sets the folder name for display.
    /// </summary>
    [Display(Name = "Upload to Folder")]
    public string? FolderName { get; set; }

    /// <summary>
    /// Gets or sets version comments.
    /// </summary>
    [StringLength(500, ErrorMessage = "Version comments cannot exceed 500 characters")]
    [Display(Name = "Version Comments")]
    public string? VersionComments { get; set; }

    /// <summary>
    /// Gets or sets whether to overwrite existing document.
    /// </summary>
    [Display(Name = "Overwrite if exists")]
    public bool OverwriteExisting { get; set; }
}

/// <summary>
/// View model for editing document properties.
/// </summary>
public class EditDocumentViewModel
{
    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the document name (read-only).
    /// </summary>
    [Display(Name = "Document Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    [StringLength(500, ErrorMessage = "Title cannot exceed 500 characters")]
    [Display(Name = "Title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the document description.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the document tags.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Tags cannot exceed 1000 characters")]
    [Display(Name = "Tags")]
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the current folder identifier.
    /// </summary>
    public Guid CurrentFolderId { get; set; }

    /// <summary>
    /// Gets or sets the current folder name for display.
    /// </summary>
    [Display(Name = "Current Folder")]
    public string CurrentFolderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new folder identifier.
    /// </summary>
    [Display(Name = "Move to Folder")]
    public Guid? NewFolderId { get; set; }
}
