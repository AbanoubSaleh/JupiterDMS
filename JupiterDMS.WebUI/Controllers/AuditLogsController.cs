using JupiterDMS.WebUI.Models;
using JupiterDMS.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JupiterDMS.WebUI.Controllers;

/// <summary>
/// MVC controller for viewing audit logs in the UI (Admin only).
/// </summary>
[Authorize(Roles = "Admin")]
public class AuditLogsController : Controller
{
    private readonly JupiterDmsApiClient _apiClient;
    private readonly ILogger<AuditLogsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogsController"/> class.
    /// </summary>
    /// <param name="apiClient">The API client.</param>
    /// <param name="logger">The logger.</param>
    public AuditLogsController(JupiterDmsApiClient apiClient, ILogger<AuditLogsController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Displays the audit logs with filtering.
    /// </summary>
    /// <param name="filter">The filter criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The audit logs view.</returns>
    public async Task<IActionResult> Index(AuditLogFilterViewModel? filter = null, CancellationToken cancellationToken = default)
    {
        filter ??= new AuditLogFilterViewModel();

        // Set default date range if not specified (last 30 days)
        if (!filter.FromDate.HasValue && !filter.ToDate.HasValue)
        {
            filter.ToDate = DateTime.Now;
            filter.FromDate = DateTime.Now.AddDays(-30);
        }

        try
        {
            var results = await _apiClient.GetAuditLogsAsync(filter, cancellationToken);
            
            if (results != null)
            {
                // Load filter options
                await LoadFilterOptions(cancellationToken);
                return View(results);
            }

            // Return empty results on error
            var emptyResults = new AuditLogResultsViewModel
            {
                Filter = filter,
                AuditLogs = new List<AuditLogViewModel>(),
                TotalResults = 0,
                CurrentPage = 1,
                PageSize = filter.PageSize,
                TotalPages = 0,
                HasNextPage = false,
                HasPreviousPage = false
            };

            await LoadFilterOptions(cancellationToken);
            return View(emptyResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            
            var emptyResults = new AuditLogResultsViewModel
            {
                Filter = filter,
                AuditLogs = new List<AuditLogViewModel>(),
                TotalResults = 0,
                CurrentPage = 1,
                PageSize = filter.PageSize,
                TotalPages = 0,
                HasNextPage = false,
                HasPreviousPage = false
            };

            await LoadFilterOptions(cancellationToken);
            TempData["ErrorMessage"] = "An error occurred while retrieving audit logs";
            return View(emptyResults);
        }
    }

    /// <summary>
    /// Filters audit logs with POST request.
    /// </summary>
    /// <param name="filter">The filter criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The audit logs view with filtered results.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Filter(AuditLogFilterViewModel filter, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            await LoadFilterOptions(cancellationToken);
            
            var emptyResults = new AuditLogResultsViewModel
            {
                Filter = filter,
                AuditLogs = new List<AuditLogViewModel>(),
                TotalResults = 0,
                CurrentPage = 1,
                PageSize = filter.PageSize,
                TotalPages = 0,
                HasNextPage = false,
                HasPreviousPage = false
            };

            return View("Index", emptyResults);
        }

        return await Index(filter, cancellationToken);
    }

    /// <summary>
    /// Exports audit logs to CSV.
    /// </summary>
    /// <param name="filter">The filter criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>CSV file download.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportCsv(AuditLogFilterViewModel filter, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all results (not paginated) for export
            filter.PageSize = 10000; // Large page size to get all results
            filter.PageNumber = 1;

            var results = await _apiClient.GetAuditLogsAsync(filter, cancellationToken);
            
            if (results?.AuditLogs == null || !results.AuditLogs.Any())
            {
                TempData["ErrorMessage"] = "No audit logs to export";
                return RedirectToAction(nameof(Index));
            }

            var csv = GenerateCsv(results.AuditLogs);
            var fileName = $"audit_logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting audit logs to CSV");
            TempData["ErrorMessage"] = "An error occurred while exporting audit logs";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Gets audit logs for a specific entity.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityId">The entity identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The audit logs view filtered by entity.</returns>
    public async Task<IActionResult> ForEntity(string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        var filter = new AuditLogFilterViewModel
        {
            EntityType = entityType,
            EntityId = entityId,
            PageSize = 50,
            SortBy = "Timestamp",
            SortDescending = true
        };

        return await Index(filter, cancellationToken);
    }

    /// <summary>
    /// Gets audit logs for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The audit logs view filtered by user.</returns>
    public async Task<IActionResult> ForUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var filter = new AuditLogFilterViewModel
        {
            PerformedBy = userId,
            PageSize = 50,
            SortBy = "Timestamp",
            SortDescending = true
        };

        return await Index(filter, cancellationToken);
    }

    /// <summary>
    /// Loads filter options for dropdowns.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    private async Task LoadFilterOptions(CancellationToken cancellationToken)
    {
        try
        {
            var users = await _apiClient.GetUsersAsync(cancellationToken);
            ViewBag.Users = users ?? Enumerable.Empty<UserViewModel>();

            ViewBag.ActionTypes = new[]
            {
                new { Value = "Create", Text = "Create" },
                new { Value = "Update", Text = "Update" },
                new { Value = "Delete", Text = "Delete" },
                new { Value = "Download", Text = "Download" },
                new { Value = "CheckOut", Text = "Check Out" },
                new { Value = "CheckIn", Text = "Check In" },
                new { Value = "Login", Text = "Login" },
                new { Value = "Logout", Text = "Logout" },
                new { Value = "Search", Text = "Search" },
                new { Value = "Move", Text = "Move" },
                new { Value = "Copy", Text = "Copy" },
                new { Value = "Rename", Text = "Rename" }
            };

            ViewBag.EntityTypes = new[]
            {
                new { Value = "Document", Text = "Document" },
                new { Value = "Folder", Text = "Folder" },
                new { Value = "Library", Text = "Library" },
                new { Value = "User", Text = "User" }
            };

            ViewBag.SortOptions = new[]
            {
                new { Value = "Timestamp", Text = "Timestamp" },
                new { Value = "ActionType", Text = "Action Type" },
                new { Value = "EntityType", Text = "Entity Type" },
                new { Value = "PerformedByUsername", Text = "User" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading filter options");
            // Set empty collections to prevent view errors
            ViewBag.Users = Enumerable.Empty<UserViewModel>();
            ViewBag.ActionTypes = Array.Empty<object>();
            ViewBag.EntityTypes = Array.Empty<object>();
            ViewBag.SortOptions = Array.Empty<object>();
        }
    }

    /// <summary>
    /// Generates CSV content from audit logs.
    /// </summary>
    /// <param name="auditLogs">The audit logs to export.</param>
    /// <returns>CSV content as string.</returns>
    private static string GenerateCsv(IEnumerable<AuditLogViewModel> auditLogs)
    {
        var csv = new System.Text.StringBuilder();
        
        // Header
        csv.AppendLine("Timestamp,Action,Entity Type,Entity,User,Details,IP Address");
        
        // Data rows
        foreach (var log in auditLogs)
        {
            csv.AppendLine($"\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{log.ActionType}\",\"{log.EntityType}\",\"{EscapeCsv(log.EntityName)}\",\"{EscapeCsv(log.PerformedByFullName)}\",\"{EscapeCsv(log.Details)}\",\"{log.IpAddress}\"");
        }
        
        return csv.ToString();
    }

    /// <summary>
    /// Escapes CSV field content.
    /// </summary>
    /// <param name="field">The field content.</param>
    /// <returns>Escaped field content.</returns>
    private static string EscapeCsv(string? field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;
            
        return field.Replace("\"", "\"\"");
    }
}
