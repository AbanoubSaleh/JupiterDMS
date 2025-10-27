using System.ComponentModel.DataAnnotations;

namespace JupiterDMS.Application.Common.DTOs;

/// <summary>
/// Data transfer object for updating a folder.
/// Excludes auto-generated fields like CreatedOn, ModifiedOn, CreatedBy, ModifiedBy, Path.
/// </summary>
public class UpdateFolderDto
{
    /// <summary>
    /// Gets or sets the folder ID.
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the folder name.
    /// </summary>
    [Required(ErrorMessage = "Folder name is required.")]
    [StringLength(255, ErrorMessage = "Folder name cannot exceed 255 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder description.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string? Description { get; set; }
}
