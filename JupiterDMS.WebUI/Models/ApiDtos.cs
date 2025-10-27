namespace JupiterDMS.WebUI.Models;

/// <summary>
/// DTO for document version information.
/// </summary>
public class DocumentVersionViewModel
{
    /// <summary>
    /// Gets or sets the version number.
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Gets or sets the file size.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the version comments.
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Gets or sets when the version was created.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets who created the version.
    /// </summary>
    public string? CreatedByUsername { get; set; }

    /// <summary>
    /// Gets the formatted file size.
    /// </summary>
    public string FormattedSize => FormatFileSize(Size);

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
/// Result for document download operations.
/// </summary>
public class DocumentDownloadResult
{
    /// <summary>
    /// Gets or sets the file content.
    /// </summary>
    public byte[] FileContent { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
}

/// <summary>
/// DTO for search results.
/// </summary>
public class SearchResultsDto
{
    /// <summary>
    /// Gets or sets the documents.
    /// </summary>
    public IEnumerable<DocumentViewModel> Documents { get; set; } = Enumerable.Empty<DocumentViewModel>();

    /// <summary>
    /// Gets or sets the total number of results.
    /// </summary>
    public int TotalResults { get; set; }

    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Gets whether there is a next page.
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Gets whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage { get; set; }
}
