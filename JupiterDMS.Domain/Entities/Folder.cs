using JupiterDMS.Domain.Common;

namespace JupiterDMS.Domain.Entities;

/// <summary>
/// Represents a folder in the document management system.
/// </summary>
public class Folder : BaseEntity
{
    /// <summary>
    /// Gets or sets the name of the folder.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full path of the folder.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the folder.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the library identifier this folder belongs to.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the parent folder identifier (null for root folders).
    /// </summary>
    public Guid? ParentFolderId { get; set; }

    /// <summary>
    /// Gets or sets the library this folder belongs to.
    /// </summary>
    public virtual Library Library { get; set; } = null!;

    /// <summary>
    /// Gets or sets the parent folder (null for root folders).
    /// </summary>
    public virtual Folder? ParentFolder { get; set; }

    /// <summary>
    /// Gets or sets the collection of child folders.
    /// </summary>
    public virtual ICollection<Folder> ChildFolders { get; set; } = new List<Folder>();

    /// <summary>
    /// Gets or sets the collection of documents in this folder.
    /// </summary>
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}

