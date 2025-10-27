using System.ComponentModel.DataAnnotations;

namespace JupiterDMS.Application.Common.DTOs;

/// <summary>
/// Data transfer object for library responses.
/// </summary>
public class LibraryDto
{
    /// <summary>
    /// Gets or sets the library ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the library name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the library description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether the library is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets when the library was created.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets who created the library.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets when the library was last modified.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets who last modified the library.
    /// </summary>
    public Guid? ModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets whether the library is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
}
