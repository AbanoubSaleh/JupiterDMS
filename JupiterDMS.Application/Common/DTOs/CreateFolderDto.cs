using System.ComponentModel.DataAnnotations;

namespace JupiterDMS.Application.Common.DTOs;

/// <summary>
/// Data transfer object for creating a folder.
/// Excludes auto-generated fields like Id, CreatedOn, ModifiedOn, CreatedBy, ModifiedBy.
/// </summary>
public class CreateFolderDto
{
    /// <summary>
    /// Gets or sets the folder name.
    /// </summary>
    [Required(ErrorMessage = "Folder name is required.")]
    [StringLength(255, ErrorMessage = "Folder name cannot exceed 255 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the library ID.
    /// </summary>
    [Required(ErrorMessage = "Library ID is required.")]
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the parent folder ID (optional for root folders).
    /// </summary>
    public Guid? ParentFolderId { get; set; }

    /// <summary>
    /// Gets or sets the folder description.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string? Description { get; set; }
}
