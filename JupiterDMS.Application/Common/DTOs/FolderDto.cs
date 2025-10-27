using System.ComponentModel.DataAnnotations;

namespace JupiterDMS.Application.Common.DTOs;

/// <summary>
/// Data transfer object for folder responses.
/// </summary>
public class FolderDto
{
    /// <summary>
    /// Gets or sets the folder ID.
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
    /// Gets or sets the folder description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the library ID.
    /// </summary>
    public Guid LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the parent folder ID.
    /// </summary>
    public Guid? ParentFolderId { get; set; }

    /// <summary>
    /// Gets or sets when the folder was created.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets who created the folder.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets when the folder was last modified.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets who last modified the folder.
    /// </summary>
    public Guid? ModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets whether the folder is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
}
