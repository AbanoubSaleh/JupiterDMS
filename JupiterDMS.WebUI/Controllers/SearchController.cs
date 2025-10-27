using JupiterDMS.WebUI.Models;
using JupiterDMS.WebUI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JupiterDMS.WebUI.Controllers;

/// <summary>
/// MVC controller for document search in the UI.
/// </summary>
public class SearchController : Controller
{
    private readonly JupiterDmsApiClient _apiClient;
    private readonly ILogger<SearchController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchController"/> class.
    /// </summary>
    /// <param name="apiClient">The API client.</param>
    /// <param name="logger">The logger.</param>
    public SearchController(JupiterDmsApiClient apiClient, ILogger<SearchController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Displays the search form.
    /// </summary>
    /// <param name="q">The quick search query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The search view.</returns>
    public async Task<IActionResult> Index(string? q = null, CancellationToken cancellationToken = default)
    {
        var model = new SearchViewModel
        {
            Query = q
        };

        // If there's a quick search query, perform the search immediately
        if (!string.IsNullOrEmpty(q))
        {
            return await Search(model, cancellationToken);
        }

        // Load libraries and users for filter dropdowns
        await LoadSearchFilters(cancellationToken);
        
        return View(model);
    }

    /// <summary>
    /// Performs a document search.
    /// </summary>
    /// <param name="model">The search criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The search results view.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Search(SearchViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            await LoadSearchFilters(cancellationToken);
            return View("Index", model);
        }

        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            var searchResults = await _apiClient.SearchDocumentsAsync(model, cancellationToken);
            
            stopwatch.Stop();

            var resultsModel = new SearchResultsViewModel
            {
                SearchCriteria = model,
                Documents = searchResults?.Documents?.ToList() ?? new List<DocumentViewModel>(),
                TotalResults = searchResults?.TotalResults ?? 0,
                CurrentPage = searchResults?.CurrentPage ?? 1,
                PageSize = searchResults?.PageSize ?? model.PageSize,
                TotalPages = searchResults?.TotalPages ?? 0,
                HasNextPage = searchResults?.HasNextPage ?? false,
                HasPreviousPage = searchResults?.HasPreviousPage ?? false,
                SearchTime = stopwatch.Elapsed
            };

            // Load filters for the search form
            await LoadSearchFilters(cancellationToken);

            return View("Results", resultsModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing document search");
            ModelState.AddModelError(string.Empty, "An error occurred while searching documents");
            
            await LoadSearchFilters(cancellationToken);
            return View("Index", model);
        }
    }

    /// <summary>
    /// Performs a search with GET parameters (for direct links).
    /// </summary>
    /// <param name="q">The search query.</param>
    /// <param name="libraryId">The library filter.</param>
    /// <param name="folderId">The folder filter.</param>
    /// <param name="fileType">The file type filter.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="sortBy">The sort field.</param>
    /// <param name="sortDesc">Whether to sort descending.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The search results view.</returns>
    [HttpGet]
    public async Task<IActionResult> Results(
        string? q = null,
        Guid? libraryId = null,
        Guid? folderId = null,
        string? fileType = null,
        int page = 1,
        int pageSize = 20,
        string sortBy = "CreatedOn",
        bool sortDesc = true,
        CancellationToken cancellationToken = default)
    {
        var model = new SearchViewModel
        {
            Query = q,
            LibraryId = libraryId,
            FolderId = folderId,
            FileType = fileType,
            PageNumber = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDesc
        };

        return await Search(model, cancellationToken);
    }

    /// <summary>
    /// Gets search suggestions for autocomplete.
    /// </summary>
    /// <param name="term">The search term.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON array of suggestions.</returns>
    [HttpGet]
    public async Task<IActionResult> Suggestions(string term, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(Array.Empty<string>());
            }

            var suggestions = await _apiClient.GetSearchSuggestionsAsync(term, cancellationToken);
            return Json(suggestions ?? Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search suggestions for term: {Term}", term);
            return Json(Array.Empty<string>());
        }
    }

    /// <summary>
    /// Exports search results to CSV.
    /// </summary>
    /// <param name="model">The search criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>CSV file download.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportCsv(SearchViewModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all results (not paginated) for export
            model.PageSize = 10000; // Large page size to get all results
            model.PageNumber = 1;

            var searchResults = await _apiClient.SearchDocumentsAsync(model, cancellationToken);
            
            if (searchResults?.Documents == null || !searchResults.Documents.Any())
            {
                TempData["ErrorMessage"] = "No results to export";
                return RedirectToAction(nameof(Index));
            }

            var csv = GenerateCsv(searchResults.Documents);
            var fileName = $"search_results_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting search results to CSV");
            TempData["ErrorMessage"] = "An error occurred while exporting results";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Loads filter options for the search form.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    private async Task LoadSearchFilters(CancellationToken cancellationToken)
    {
        try
        {
            var libraries = await _apiClient.GetLibrariesAsync(cancellationToken: cancellationToken);
            var users = await _apiClient.GetUsersAsync(cancellationToken);

            ViewBag.Libraries = libraries ?? Enumerable.Empty<LibraryViewModel>();
            ViewBag.Users = users ?? Enumerable.Empty<UserViewModel>();
            
            ViewBag.FileTypes = new[]
            {
                new { Value = "Word", Text = "Word Documents" },
                new { Value = "Excel", Text = "Excel Spreadsheets" },
                new { Value = "PowerPoint", Text = "PowerPoint Presentations" },
                new { Value = "Pdf", Text = "PDF Documents" },
                new { Value = "Image", Text = "Images" },
                new { Value = "Text", Text = "Text Files" },
                new { Value = "Other", Text = "Other Files" }
            };

            ViewBag.CheckoutStatuses = new[]
            {
                new { Value = "Available", Text = "Available" },
                new { Value = "CheckedOut", Text = "Checked Out" },
                new { Value = "Locked", Text = "Locked" }
            };

            ViewBag.SortOptions = new[]
            {
                new { Value = "Name", Text = "Name" },
                new { Value = "Title", Text = "Title" },
                new { Value = "Size", Text = "Size" },
                new { Value = "FileType", Text = "File Type" },
                new { Value = "CreatedOn", Text = "Created Date" },
                new { Value = "ModifiedOn", Text = "Modified Date" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading search filters");
            // Set empty collections to prevent view errors
            ViewBag.Libraries = Enumerable.Empty<LibraryViewModel>();
            ViewBag.Users = Enumerable.Empty<UserViewModel>();
            ViewBag.FileTypes = Array.Empty<object>();
            ViewBag.CheckoutStatuses = Array.Empty<object>();
            ViewBag.SortOptions = Array.Empty<object>();
        }
    }

    /// <summary>
    /// Generates CSV content from search results.
    /// </summary>
    /// <param name="documents">The documents to export.</param>
    /// <returns>CSV content as string.</returns>
    private static string GenerateCsv(IEnumerable<DocumentViewModel> documents)
    {
        var csv = new System.Text.StringBuilder();
        
        // Header
        csv.AppendLine("Name,Title,Description,File Type,Size,Library,Folder,Created On,Created By,Modified On,Modified By,Status");
        
        // Data rows
        foreach (var doc in documents)
        {
            csv.AppendLine($"\"{EscapeCsv(doc.Name)}\",\"{EscapeCsv(doc.Title)}\",\"{EscapeCsv(doc.Description)}\",\"{doc.FileType}\",\"{doc.FormattedSize}\",\"{EscapeCsv(doc.LibraryName)}\",\"{EscapeCsv(doc.FolderName)}\",\"{doc.CreatedOn:yyyy-MM-dd HH:mm}\",\"{EscapeCsv(doc.CreatedByUsername)}\",\"{doc.ModifiedOn:yyyy-MM-dd HH:mm}\",\"{EscapeCsv(doc.ModifiedByUsername)}\",\"{doc.CheckoutStatus}\"");
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
