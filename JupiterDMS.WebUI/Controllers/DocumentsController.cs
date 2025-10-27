using JupiterDMS.WebUI.Models;
using JupiterDMS.WebUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace JupiterDMS.WebUI.Controllers;

/// <summary>
/// MVC controller for managing documents in the UI.
/// </summary>
public class DocumentsController : Controller
{
    private readonly JupiterDmsApiClient _apiClient;
    private readonly ILogger<DocumentsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentsController"/> class.
    /// </summary>
    /// <param name="apiClient">The API client.</param>
    /// <param name="logger">The logger.</param>
    public DocumentsController(JupiterDmsApiClient apiClient, ILogger<DocumentsController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Displays the details of a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The view with document details.</returns>
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _apiClient.GetDocumentAsync(id, cancellationToken);
            if (document == null)
            {
                return NotFound("Document not found");
            }

            var versions = await _apiClient.GetDocumentVersionsAsync(id, cancellationToken);
            ViewBag.Versions = versions ?? Enumerable.Empty<DocumentVersionViewModel>();

            return View(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document {DocumentId}", id);
            return NotFound();
        }
    }

    /// <summary>
    /// Displays the upload document form.
    /// </summary>
    /// <param name="folderId">The folder identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The upload view.</returns>
    public async Task<IActionResult> Upload(Guid folderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var folder = await _apiClient.GetFolderAsync(folderId, cancellationToken);
            if (folder == null)
            {
                return NotFound("Folder not found");
            }

            var model = new UploadDocumentViewModel
            {
                FolderId = folderId,
                FolderName = folder.Name
            };

            ViewBag.Folder = folder;
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing upload form for folder {FolderId}", folderId);
            return RedirectToAction("Details", "Folders", new { id = folderId });
        }
    }

    /// <summary>
    /// Uploads a document.
    /// </summary>
    /// <param name="model">The upload document model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect to folder on success; otherwise, the upload view.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(UploadDocumentViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _apiClient.UploadDocumentAsync(model, cancellationToken);

            if (result != null)
            {
                TempData["SuccessMessage"] = "Document uploaded successfully";
                return RedirectToAction("Details", "Folders", new { id = model.FolderId });
            }

            ModelState.AddModelError(string.Empty, "Failed to upload document");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            ModelState.AddModelError(string.Empty, "An error occurred while uploading the document");
            return View(model);
        }
    }

    /// <summary>
    /// Downloads a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="versionNumber">The version number to download.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file download.</returns>
    public async Task<IActionResult> Download(Guid id, int? versionNumber = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var downloadResult = await _apiClient.DownloadDocumentAsync(id, versionNumber, cancellationToken);
            
            if (downloadResult == null)
            {
                return NotFound("Document not found");
            }

            return File(downloadResult.FileContent, downloadResult.ContentType, downloadResult.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {DocumentId}", id);
            return NotFound();
        }
    }

    /// <summary>
    /// Displays the edit document form.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The edit view.</returns>
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _apiClient.GetDocumentAsync(id, cancellationToken);
            if (document == null)
            {
                return NotFound("Document not found");
            }

            var model = new EditDocumentViewModel
            {
                Id = document.Id,
                Name = document.Name,
                Title = document.Title,
                Description = document.Description,
                Tags = document.Tags,
                CurrentFolderId = document.FolderId,
                CurrentFolderName = document.FolderName
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document {DocumentId} for editing", id);
            return NotFound();
        }
    }

    /// <summary>
    /// Updates a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="model">The edit document model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect to document details on success; otherwise, the edit view.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditDocumentViewModel model, CancellationToken cancellationToken = default)
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
            var success = await _apiClient.UpdateDocumentAsync(id, model, cancellationToken);

            if (success)
            {
                TempData["SuccessMessage"] = "Document updated successfully";
                return RedirectToAction(nameof(Details), new { id });
            }

            ModelState.AddModelError(string.Empty, "Failed to update document");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document {DocumentId}", id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the document");
            return View(model);
        }
    }

    /// <summary>
    /// Checks out a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="hours">The checkout hours.</param>
    /// <param name="comments">The checkout comments.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect to document details.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(Guid id, int? hours = null, string? comments = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _apiClient.CheckOutDocumentAsync(id, hours, comments, cancellationToken);

            if (success)
            {
                TempData["SuccessMessage"] = "Document checked out successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to check out document";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking out document {DocumentId}", id);
            TempData["ErrorMessage"] = "An error occurred while checking out the document";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    /// <summary>
    /// Checks in a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="updatedFile">The updated file.</param>
    /// <param name="comments">The check-in comments.</param>
    /// <param name="keepCheckedOut">Whether to keep checked out.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect to document details.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(Guid id, IFormFile? updatedFile = null, string? comments = null, bool keepCheckedOut = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _apiClient.CheckInDocumentAsync(id, updatedFile, comments, keepCheckedOut, cancellationToken);

            if (success)
            {
                TempData["SuccessMessage"] = "Document checked in successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to check in document";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in document {DocumentId}", id);
            TempData["ErrorMessage"] = "An error occurred while checking in the document";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    /// <summary>
    /// Displays the delete document confirmation.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The delete confirmation view.</returns>
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _apiClient.GetDocumentAsync(id, cancellationToken);
            if (document == null)
            {
                return NotFound("Document not found");
            }

            return View(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document {DocumentId} for deletion", id);
            return NotFound();
        }
    }

    /// <summary>
    /// Deletes a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="permanentDelete">Whether to permanently delete.</param>
    /// <param name="reason">The deletion reason.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect to folder.</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, bool permanentDelete = false, string? reason = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _apiClient.GetDocumentAsync(id, cancellationToken);
            if (document == null)
            {
                return NotFound();
            }

            var success = await _apiClient.DeleteDocumentAsync(id, permanentDelete, reason, cancellationToken);

            if (success)
            {
                TempData["SuccessMessage"] = "Document deleted successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete document";
            }

            return RedirectToAction("Details", "Folders", new { id = document.FolderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the document";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
