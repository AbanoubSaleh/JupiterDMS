using System.ComponentModel.DataAnnotations;

namespace JupiterDMS.WebUI.Models;

/// <summary>
/// View model for library display.
/// </summary>
public class LibraryViewModel
{
    /// <summary>
    /// Gets or sets the library identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the library name.
    /// </summary>
    [Display(Name = "Library Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the library description.
    /// </summary>
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets when the library was created.
    /// </summary>
    [Display(Name = "Created On")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets who created the library.
    /// </summary>
    [Display(Name = "Created By")]
    public string? CreatedByUsername { get; set; }

    /// <summary>
    /// Gets or sets when the library was last modified.
    /// </summary>
    [Display(Name = "Modified On")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets who last modified the library.
    /// </summary>
    [Display(Name = "Modified By")]
    public string? ModifiedByUsername { get; set; }

    /// <summary>
    /// Gets or sets the number of folders in the library.
    /// </summary>
    [Display(Name = "Folders")]
    public int FolderCount { get; set; }

    /// <summary>
    /// Gets or sets the number of documents in the library.
    /// </summary>
    [Display(Name = "Documents")]
    public int DocumentCount { get; set; }

    /// <summary>
    /// Gets or sets whether the library is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
}

/// <summary>
/// View model for creating a library.
/// </summary>
public class CreateLibraryViewModel
{
    /// <summary>
    /// Gets or sets the library name.
    /// </summary>
    [Required(ErrorMessage = "Library name is required")]
    [StringLength(100, ErrorMessage = "Library name cannot exceed 100 characters")]
    [Display(Name = "Library Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the library description.
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets who is creating the library.
    /// </summary>
    public Guid CreatedBy { get; set; }
}

/// <summary>
/// View model for editing a library.
/// </summary>
public class EditLibraryViewModel
{
    /// <summary>
    /// Gets or sets the library identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the library name.
    /// </summary>
    [Required(ErrorMessage = "Library name is required")]
    [StringLength(100, ErrorMessage = "Library name cannot exceed 100 characters")]
    [Display(Name = "Library Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the library description.
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }
}
