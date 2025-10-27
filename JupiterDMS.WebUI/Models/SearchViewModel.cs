using System.ComponentModel.DataAnnotations;

namespace JupiterDMS.WebUI.Models;

/// <summary>
/// View model for document search.
/// </summary>
public class SearchViewModel
{
    /// <summary>
    /// Gets or sets the search query.
    /// </summary>
    [Display(Name = "Search")]
    [StringLength(500, ErrorMessage = "Search query cannot exceed 500 characters")]
    public string? Query { get; set; }

    /// <summary>
    /// Gets or sets the library filter.
    /// </summary>
    [Display(Name = "Library")]
    public Guid? LibraryId { get; set; }

    /// <summary>
    /// Gets or sets the folder filter.
    /// </summary>
    [Display(Name = "Folder")]
    public Guid? FolderId { get; set; }

    /// <summary>
    /// Gets or sets whether to include subfolders.
    /// </summary>
    [Display(Name = "Include Subfolders")]
    public bool IncludeSubfolders { get; set; } = true;

    /// <summary>
    /// Gets or sets the file type filter.
    /// </summary>
    [Display(Name = "File Type")]
    public string? FileType { get; set; }

    /// <summary>
    /// Gets or sets the created by filter.
    /// </summary>
    [Display(Name = "Created By")]
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the modified by filter.
    /// </summary>
    [Display(Name = "Modified By")]
    public Guid? ModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the created from date filter.
    /// </summary>
    [Display(Name = "Created From")]
    [DataType(DataType.Date)]
    public DateTime? CreatedFrom { get; set; }

    /// <summary>
    /// Gets or sets the created to date filter.
    /// </summary>
    [Display(Name = "Created To")]
    [DataType(DataType.Date)]
    public DateTime? CreatedTo { get; set; }

    /// <summary>
    /// Gets or sets the modified from date filter.
    /// </summary>
    [Display(Name = "Modified From")]
    [DataType(DataType.Date)]
    public DateTime? ModifiedFrom { get; set; }

    /// <summary>
    /// Gets or sets the modified to date filter.
    /// </summary>
    [Display(Name = "Modified To")]
    [DataType(DataType.Date)]
    public DateTime? ModifiedTo { get; set; }

    /// <summary>
    /// Gets or sets the minimum file size filter.
    /// </summary>
    [Display(Name = "Min Size (MB)")]
    [Range(0, int.MaxValue, ErrorMessage = "Minimum size must be greater than or equal to 0")]
    public decimal? MinSizeMB { get; set; }

    /// <summary>
    /// Gets or sets the maximum file size filter.
    /// </summary>
    [Display(Name = "Max Size (MB)")]
    [Range(0, int.MaxValue, ErrorMessage = "Maximum size must be greater than or equal to 0")]
    public decimal? MaxSizeMB { get; set; }

    /// <summary>
    /// Gets or sets the checkout status filter.
    /// </summary>
    [Display(Name = "Checkout Status")]
    public string? CheckoutStatus { get; set; }

    /// <summary>
    /// Gets or sets the sort field.
    /// </summary>
    [Display(Name = "Sort By")]
    public string SortBy { get; set; } = "CreatedOn";

    /// <summary>
    /// Gets or sets whether to sort descending.
    /// </summary>
    [Display(Name = "Sort Descending")]
    public bool SortDescending { get; set; } = true;

    /// <summary>
    /// Gets or sets the page number.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    [Display(Name = "Results Per Page")]
    [Range(10, 100, ErrorMessage = "Page size must be between 10 and 100")]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Gets the minimum size in bytes.
    /// </summary>
    public long? MinSize => MinSizeMB.HasValue ? (long)(MinSizeMB.Value * 1024 * 1024) : null;

    /// <summary>
    /// Gets the maximum size in bytes.
    /// </summary>
    public long? MaxSize => MaxSizeMB.HasValue ? (long)(MaxSizeMB.Value * 1024 * 1024) : null;
}

/// <summary>
/// View model for search results.
/// </summary>
public class SearchResultsViewModel
{
    /// <summary>
    /// Gets or sets the search criteria.
    /// </summary>
    public SearchViewModel SearchCriteria { get; set; } = new();

    /// <summary>
    /// Gets or sets the search results.
    /// </summary>
    public List<DocumentViewModel> Documents { get; set; } = new();

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

    /// <summary>
    /// Gets the search execution time.
    /// </summary>
    public TimeSpan? SearchTime { get; set; }

    /// <summary>
    /// Gets whether there are any results.
    /// </summary>
    public bool HasResults => Documents.Any();

    /// <summary>
    /// Gets the result range text for display.
    /// </summary>
    public string ResultRangeText
    {
        get
        {
            if (TotalResults == 0) return "No results";
            
            var start = (CurrentPage - 1) * PageSize + 1;
            var end = Math.Min(CurrentPage * PageSize, TotalResults);
            return $"Showing {start}-{end} of {TotalResults} results";
        }
    }
}
