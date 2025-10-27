using JupiterDMS.Domain.Common;

namespace JupiterDMS.Domain.Entities;

/// <summary>
/// Represents a library (top-level container) in the document management system.
/// </summary>
public class Library : BaseEntity
{
    /// <summary>
    /// Gets or sets the name of the library.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the library.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the library is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the collection of folders in this library.
    /// </summary>
    public virtual ICollection<Folder> Folders { get; set; } = new List<Folder>();
}

