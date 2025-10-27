using JupiterDMS.WebUI.Models;
using JupiterDMS.WebUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace JupiterDMS.WebUI.Controllers;

/// <summary>
/// MVC controller for managing folders in the UI.
/// </summary>
public class FoldersController : Controller
{
    private readonly JupiterDmsApiClient _apiClient;
    private readonly ILogger<FoldersController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FoldersController"/> class.
    /// </summary>
    /// <param name="apiClient">The API client.</param>
    /// <param name="logger">The logger.</param>
    public FoldersController(JupiterDmsApiClient apiClient, ILogger<FoldersController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Displays the folder tree for a library.
    /// </summary>
    /// <param name="libraryId">The library identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The view with folder tree.</returns>
    public async Task<IActionResult> Index(Guid libraryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var library = await _apiClient.GetLibraryAsync(libraryId, cancellationToken);
            if (library == null)
            {
                return NotFound("Library not found");
            }

            var folders = await _apiClient.GetFolderTreeAsync(libraryId, cancellationToken);
            
            ViewBag.Library = library;
            return View(folders ?? Enumerable.Empty<FolderTreeViewModel>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving folder tree for library {LibraryId}", libraryId);
            return View(Enumerable.Empty<FolderTreeViewModel>());
        }
    }

    /// <summary>
    /// Displays the details of a folder.
    /// </summary>
    /// <param name="id">The folder identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The view with folder details.</returns>
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var folder = await _apiClient.GetFolderAsync(id, cancellationToken);
            if (folder == null)
            {
                return NotFound("Folder not found");
            }

            var documents = await _apiClient.GetDocumentsByFolderAsync(id, cancellationToken: cancellationToken);
            var subfolders = await _apiClient.GetSubfoldersAsync(id, cancellationToken);

            ViewBag.Documents = documents ?? Enumerable.Empty<DocumentViewModel>();
            ViewBag.Subfolders = subfolders ?? Enumerable.Empty<FolderViewModel>();
            
            return View(folder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving folder {FolderId}", id);
            return NotFound();
        }
    }

    /// <summary>
    /// Displays the create folder form.
    /// </summary>
    /// <param name="libraryId">The library identifier.</param>
    /// <param name="parentFolderId">The parent folder identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The create view.</returns>
    public async Task<IActionResult> Create(Guid libraryId, Guid? parentFolderId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var library = await _apiClient.GetLibraryAsync(libraryId, cancellationToken);
            if (library == null)
            {
                return NotFound("Library not found");
            }

            var model = new CreateFolderViewModel
            {
                LibraryId = libraryId,
                ParentFolderId = parentFolderId
            };

            ViewBag.Library = library;
            
            if (parentFolderId.HasValue)
            {
                var parentFolder = await _apiClient.GetFolderAsync(parentFolderId.Value, cancellationToken);
                ViewBag.ParentFolder = parentFolder;
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing create folder form");
            return RedirectToAction("Index", "Libraries");
        }
    }

    /// <summary>
    /// Creates a new folder.
    /// </summary>
    /// <param name="model">The create folder model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect to folder tree on success; otherwise, the create view.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFolderViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            model.CreatedBy = Guid.NewGuid(); // TODO: Get from current user
            var result = await _apiClient.CreateFolderAsync(model, cancellationToken);

            if (result != null)
            {
                TempData["SuccessMessage"] = "Folder created successfully";
                return RedirectToAction(nameof(Index), new { libraryId = model.LibraryId });
            }

            ModelState.AddModelError(string.Empty, "Failed to create folder");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating folder");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the folder");
            return View(model);
        }
    }

    /// <summary>
    /// Displays the edit folder form.
    /// </summary>
    /// <param name="id">The folder identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The edit view.</returns>
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var folder = await _apiClient.GetFolderAsync(id, cancellationToken);
            if (folder == null)
            {
                return NotFound("Folder not found");
            }

            var model = new EditFolderViewModel
            {
                Id = folder.Id,
                Name = folder.Name,
                Description = folder.Description,
                CurrentPath = folder.Path
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving folder {FolderId} for editing", id);
            return NotFound();
        }
    }

    /// <summary>
    /// Updates a folder.
    /// </summary>
    /// <param name="id">The folder identifier.</param>
    /// <param name="model">The edit folder model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect to folder details on success; otherwise, the edit view.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditFolderViewModel model, CancellationToken cancellationToken = default)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var success = await _apiClient.UpdateFolderAsync(id, model, cancellationToken);

            if (success)
            {
                TempData["SuccessMessage"] = "Folder updated successfully";
                return RedirectToAction(nameof(Details), new { id });
            }

            ModelState.AddModelError(string.Empty, "Failed to update folder");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating folder {FolderId}", id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the folder");
            return View(model);
        }
    }

    /// <summary>
    /// Displays the delete folder confirmation.
    /// </summary>
    /// <param name="id">The folder identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The delete confirmation view.</returns>
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var folder = await _apiClient.GetFolderAsync(id, cancellationToken);
            if (folder == null)
            {
                return NotFound("Folder not found");
            }

            return View(folder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving folder {FolderId} for deletion", id);
            return NotFound();
        }
    }

    /// <summary>
    /// Deletes a folder.
    /// </summary>
    /// <param name="id">The folder identifier.</param>
    /// <param name="forceDelete">Whether to force delete if folder contains items.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect to folder tree.</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, bool forceDelete = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var folder = await _apiClient.GetFolderAsync(id, cancellationToken);
            if (folder == null)
            {
                return NotFound();
            }

            var success = await _apiClient.DeleteFolderAsync(id, forceDelete, cancellationToken);

            if (success)
            {
                TempData["SuccessMessage"] = "Folder deleted successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete folder";
            }

            return RedirectToAction(nameof(Index), new { libraryId = folder.LibraryId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting folder {FolderId}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the folder";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
