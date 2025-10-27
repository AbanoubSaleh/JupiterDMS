using System.ComponentModel.DataAnnotations;

namespace JupiterDMS.WebUI.Models;

/// <summary>
/// View model for folder display.
/// </summary>
public class FolderViewModel
{
    /// <summary>
    /// Gets or sets the folder identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the folder name.
    /// </summary>
    [Display(Name = "Folder Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder description.
    /// </summary>
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the folder path.
    /// </summary>
    [Display(Name = "Path")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent folder identifier.
    /// </summary>
    public Guid? ParentFolderId { get; set; }

    /// <summary>
    /// Gets or sets the parent folder name.
    /// </summary>
    [Display(Name = "Parent Folder")]
    public string? ParentFolderName { get; set; }

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
    /// Gets or sets when the folder was created.
    /// </summary>
    [Display(Name = "Created On")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets who created the folder.
    /// </summary>
    [Display(Name = "Created By")]
    public string? CreatedByUsername { get; set; }

    /// <summary>
    /// Gets or sets when the folder was last modified.
    /// </summary>
    [Display(Name = "Modified On")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets who last modified the folder.
    /// </summary>
    [Display(Name = "Modified By")]
    public string? ModifiedByUsername { get; set; }

    /// <summary>
    /// Gets or sets the number of subfolders.
    /// </summary>
    [Display(Name = "Subfolders")]
    public int SubfolderCount { get; set; }

    /// <summary>
    /// Gets or sets the number of documents.
    /// </summary>
    [Display(Name = "Documents")]
    public int DocumentCount { get; set; }

    /// <summary>
    /// Gets or sets whether the folder is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
}

/// <summary>
/// View model for folder tree display.
/// </summary>
public class FolderTreeViewModel
{
    /// <summary>
    /// Gets or sets the folder identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the folder name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent folder identifier.
    /// </summary>
    public Guid? ParentFolderId { get; set; }

    /// <summary>
    /// Gets or sets the library identifier.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the child folders.
    /// </summary>
    public List<FolderTreeViewModel> Children { get; set; } = new();

    /// <summary>
    /// Gets or sets the number of documents in this folder.
    /// </summary>
    public int DocumentCount { get; set; }

    /// <summary>
    /// Gets or sets whether the folder has children.
    /// </summary>
    public bool HasChildren => Children.Any();
}

/// <summary>
/// View model for creating a folder.
/// </summary>
public class CreateFolderViewModel
{
    /// <summary>
    /// Gets or sets the folder name.
    /// </summary>
    [Required(ErrorMessage = "Folder name is required")]
    [StringLength(100, ErrorMessage = "Folder name cannot exceed 100 characters")]
    [Display(Name = "Folder Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder description.
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the parent folder identifier.
    /// </summary>
    [Display(Name = "Parent Folder")]
    public Guid? ParentFolderId { get; set; }

    /// <summary>
    /// Gets or sets the library identifier.
    /// </summary>
    [Required(ErrorMessage = "Library is required")]
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets who is creating the folder.
    /// </summary>
    public Guid CreatedBy { get; set; }
}

/// <summary>
/// View model for editing a folder.
/// </summary>
public class EditFolderViewModel
{
    /// <summary>
    /// Gets or sets the folder identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the folder name.
    /// </summary>
    [Required(ErrorMessage = "Folder name is required")]
    [StringLength(100, ErrorMessage = "Folder name cannot exceed 100 characters")]
    [Display(Name = "Folder Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder description.
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the current path for display.
    /// </summary>
    [Display(Name = "Current Path")]
    public string CurrentPath { get; set; } = string.Empty;
}
