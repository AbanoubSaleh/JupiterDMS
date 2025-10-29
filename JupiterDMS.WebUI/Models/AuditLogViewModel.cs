using System.ComponentModel.DataAnnotations;

namespace JupiterDMS.WebUI.Models;

/// <summary>
/// View model for audit log display.
/// </summary>
public class AuditLogViewModel
{
    /// <summary>
    /// Gets or sets the audit log identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the action type.
    /// </summary>
    [Display(Name = "Action")]
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity type.
    /// </summary>
    [Display(Name = "Entity Type")]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Gets or sets the entity name.
    /// </summary>
    [Display(Name = "Entity")]
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets who performed the action.
    /// </summary>
    public string PerformedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username of who performed the action.
    /// </summary>
    [Display(Name = "Performed By")]
    public string PerformedByUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full name of who performed the action.
    /// </summary>
    [Display(Name = "User")]
    public string PerformedByFullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the action was performed.
    /// </summary>
    [Display(Name = "Timestamp")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the action details.
    /// </summary>
    [Display(Name = "Details")]
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the IP address.
    /// </summary>
    [Display(Name = "IP Address")]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent.
    /// </summary>
    [Display(Name = "User Agent")]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets the action type CSS class for display.
    /// </summary>
    public string ActionTypeCssClass => ActionType.ToLower() switch
    {
        "create" => "badge bg-success",
        "update" => "badge bg-primary",
        "delete" => "badge bg-danger",
        "download" => "badge bg-info",
        "checkout" => "badge bg-warning",
        "checkin" => "badge bg-success",
        "login" => "badge bg-primary",
        "logout" => "badge bg-secondary",
        "search" => "badge bg-info",
        _ => "badge bg-light text-dark"
    };

    /// <summary>
    /// Gets the entity type icon for display.
    /// </summary>
    public string EntityTypeIcon => EntityType.ToLower() switch
    {
        "document" => "fas fa-file",
        "folder" => "fas fa-folder",
        "library" => "fas fa-book",
        "user" => "fas fa-user",
        _ => "fas fa-circle"
    };
}

/// <summary>
/// View model for audit log filters.
/// </summary>
public class AuditLogFilterViewModel
{
    /// <summary>
    /// Gets or sets the action type filter.
    /// </summary>
    [Display(Name = "Action Type")]
    public string? ActionType { get; set; }

    /// <summary>
    /// Gets or sets the entity type filter.
    /// </summary>
    [Display(Name = "Entity Type")]
    public string? EntityType { get; set; }

    /// <summary>
    /// Gets or sets the entity identifier filter.
    /// </summary>
    [Display(Name = "Entity ID")]
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Gets or sets the user filter.
    /// </summary>
    [Display(Name = "User")]
    public Guid? PerformedBy { get; set; }

    /// <summary>
    /// Gets or sets the from date filter.
    /// </summary>
    [Display(Name = "From Date")]
    [DataType(DataType.DateTime)]
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Gets or sets the to date filter.
    /// </summary>
    [Display(Name = "To Date")]
    [DataType(DataType.DateTime)]
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Gets or sets the search term.
    /// </summary>
    [Display(Name = "Search")]
    [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Gets or sets the page number.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    [Display(Name = "Results Per Page")]
    [Range(10, 100, ErrorMessage = "Page size must be between 10 and 100")]
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Gets or sets the sort field.
    /// </summary>
    [Display(Name = "Sort By")]
    public string SortBy { get; set; } = "Timestamp";

    /// <summary>
    /// Gets or sets whether to sort descending.
    /// </summary>
    [Display(Name = "Sort Descending")]
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// View model for audit log results.
/// </summary>
public class AuditLogResultsViewModel
{
    /// <summary>
    /// Gets or sets the filter criteria.
    /// </summary>
    public AuditLogFilterViewModel Filter { get; set; } = new();

    /// <summary>
    /// Gets or sets the audit logs.
    /// </summary>
    public List<AuditLogViewModel> AuditLogs { get; set; } = new();

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
    /// Gets whether there are any results.
    /// </summary>
    public bool HasResults => AuditLogs.Any();

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
