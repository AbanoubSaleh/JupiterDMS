using JupiterDMS.WebUI.Models;
using JupiterDMS.WebUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace JupiterDMS.WebUI.Controllers;

/// <summary>
/// MVC controller for managing libraries in the UI.
/// </summary>
public class LibrariesController : Controller
{
    private readonly JupiterDmsApiClient _apiClient;
    private readonly ILogger<LibrariesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibrariesController"/> class.
    /// </summary>
    /// <param name="apiClient">The API client.</param>
    /// <param name="logger">The logger.</param>
    public LibrariesController(JupiterDmsApiClient apiClient, ILogger<LibrariesController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Displays the list of libraries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The view with libraries.</returns>
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        try
        {
            var libraries = await _apiClient.GetLibrariesAsync(cancellationToken: cancellationToken);
            return View(libraries ?? Enumerable.Empty<LibraryViewModel>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving libraries");
            return View(Enumerable.Empty<LibraryViewModel>());
        }
    }

    /// <summary>
    /// Displays the details of a library.
    /// </summary>
    /// <param name="id">The library identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The view with library details.</returns>
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var library = await _apiClient.GetLibraryAsync(id, cancellationToken);

            if (library == null)
            {
                return NotFound();
            }

            return View(library);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving library {LibraryId}", id);
            return NotFound();
        }
    }

    /// <summary>
    /// Displays the create library form.
    /// </summary>
    /// <returns>The create view.</returns>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Creates a new library.
    /// </summary>
    /// <param name="model">The create library model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect to index on success; otherwise, the create view.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateLibraryViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            model.CreatedBy = Guid.NewGuid(); // TODO: Get from current user
            var result = await _apiClient.CreateLibraryAsync(model, cancellationToken);

            if (result != null)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Failed to create library");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating library");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the library");
            return View(model);
        }
    }
}

