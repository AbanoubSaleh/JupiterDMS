using System.ComponentModel.DataAnnotations;

namespace JupiterDMS.Application.Common.DTOs;

/// <summary>
/// Data transfer object for creating a library.
/// Excludes auto-generated fields like Id, CreatedOn, ModifiedOn, CreatedBy, ModifiedBy.
/// </summary>
public class CreateLibraryDto
{
    /// <summary>
    /// Gets or sets the library name.
    /// </summary>
    [Required(ErrorMessage = "Library name is required.")]
    [StringLength(200, ErrorMessage = "Library name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the library description.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the library is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
