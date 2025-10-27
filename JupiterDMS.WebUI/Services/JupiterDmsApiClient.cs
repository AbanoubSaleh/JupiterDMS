using System.Net.Http.Json;
using System.Net.Http.Headers;
using JupiterDMS.WebUI.Models;

namespace JupiterDMS.WebUI.Services;

/// <summary>
/// Typed HTTP client for communicating with JupiterDMS API.
/// </summary>
public class JupiterDmsApiClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="JupiterDmsApiClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    public JupiterDmsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Sets the authorization token for API requests.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    public void SetAuthorizationToken(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    /// <summary>
    /// Authenticates a user with the API.
    /// </summary>
    /// <param name="model">The login model.</param>
    /// <returns>The authentication response.</returns>
    public async Task<AuthResponseViewModel?> LoginAsync(LoginViewModel model)
    {
        try
        {
            var loginRequest = new
            {
                Username = model.Username,
                Password = model.Password
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponseViewModel>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new AuthResponseViewModel
            {
                IsSuccess = false,
                ErrorMessage = errorContent
            };
        }
        catch (Exception ex)
        {
            return new AuthResponseViewModel
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Registers a new user with the API.
    /// </summary>
    /// <param name="model">The registration model.</param>
    /// <returns>The authentication response.</returns>
    public async Task<AuthResponseViewModel?> RegisterAsync(RegisterViewModel model)
    {
        try
        {
            var registerRequest = new
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Role = model.Role switch
                {
                    "Admin" => 12,
                    "Editor" => 11,
                    "Viewer" => 10,
                    _ => 10
                }
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerRequest);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponseViewModel>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new AuthResponseViewModel
            {
                IsSuccess = false,
                ErrorMessage = errorContent
            };
        }
        catch (Exception ex)
        {
            return new AuthResponseViewModel
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Gets all libraries from the API.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive libraries.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of libraries.</returns>
    public async Task<IEnumerable<LibraryViewModel>?> GetLibrariesAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"api/libraries?includeInactive={includeInactive}",
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<LibraryViewModel>>(cancellationToken: cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Gets a library by its identifier.
    /// </summary>
    /// <param name="id">The library identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The library if found; otherwise, null.</returns>
    public async Task<LibraryViewModel?> GetLibraryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/libraries/{id}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<LibraryViewModel>(cancellationToken: cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Creates a new library.
    /// </summary>
    /// <param name="model">The library model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created library.</returns>
    public async Task<LibraryViewModel?> CreateLibraryAsync(
        CreateLibraryViewModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/libraries", model, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<LibraryViewModel>(cancellationToken: cancellationToken);
        }

        return null;
    }

    #region Folders

    /// <summary>
    /// Gets the folder tree for a library.
    /// </summary>
    /// <param name="libraryId">The library identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The folder tree.</returns>
    public async Task<IEnumerable<FolderTreeViewModel>?> GetFolderTreeAsync(
        Guid libraryId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/folders/tree/{libraryId}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<FolderTreeViewModel>>(cancellationToken: cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Gets a folder by its identifier.
    /// </summary>
    /// <param name="id">The folder identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The folder if found; otherwise, null.</returns>
    public async Task<FolderViewModel?> GetFolderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/folders/{id}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<FolderViewModel>(cancellationToken: cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Gets subfolders of a folder.
    /// </summary>
    /// <param name="folderId">The parent folder identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The subfolders.</returns>
    public async Task<IEnumerable<FolderViewModel>?> GetSubfoldersAsync(
        Guid folderId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/folders/{folderId}/subfolders", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<FolderViewModel>>(cancellationToken: cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Creates a new folder.
    /// </summary>
    /// <param name="model">The folder model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created folder.</returns>
    public async Task<FolderViewModel?> CreateFolderAsync(
        CreateFolderViewModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/folders", model, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<FolderViewModel>(cancellationToken: cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Updates a folder.
    /// </summary>
    /// <param name="id">The folder identifier.</param>
    /// <param name="model">The folder model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public async Task<bool> UpdateFolderAsync(
        Guid id,
        EditFolderViewModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/folders/{id}", model, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Deletes a folder.
    /// </summary>
    /// <param name="id">The folder identifier.</param>
    /// <param name="forceDelete">Whether to force delete if folder contains items.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public async Task<bool> DeleteFolderAsync(
        Guid id,
        bool forceDelete = false,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/folders/{id}?forceDelete={forceDelete}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    #endregion

    #region Documents

    /// <summary>
    /// Gets documents in a folder.
    /// </summary>
    /// <param name="folderId">The folder identifier.</param>
    /// <param name="includeDeleted">Whether to include deleted documents.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The documents.</returns>
    public async Task<IEnumerable<DocumentViewModel>?> GetDocumentsByFolderAsync(
        Guid folderId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/documents/folder/{folderId}?includeDeleted={includeDeleted}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<DocumentViewModel>>(cancellationToken: cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Gets a document by its identifier.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document if found; otherwise, null.</returns>
    public async Task<DocumentViewModel?> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/documents/{id}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<DocumentViewModel>(cancellationToken: cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Gets document versions.
    /// </summary>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document versions.</returns>
    public async Task<IEnumerable<DocumentVersionViewModel>?> GetDocumentVersionsAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/documents/{documentId}/versions", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<DocumentVersionViewModel>>(cancellationToken: cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Uploads a document.
    /// </summary>
    /// <param name="model">The upload model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The uploaded document.</returns>
    public async Task<DocumentViewModel?> UploadDocumentAsync(
        UploadDocumentViewModel model,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();

        // Add file
        var fileContent = new StreamContent(model.File.OpenReadStream());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.File.ContentType);
        content.Add(fileContent, "File", model.File.FileName);

        // Add other properties
        content.Add(new StringContent(model.FolderId.ToString()), "FolderId");
        if (!string.IsNullOrEmpty(model.Title))
            content.Add(new StringContent(model.Title), "Title");
        if (!string.IsNullOrEmpty(model.Description))
            content.Add(new StringContent(model.Description), "Description");
        if (!string.IsNullOrEmpty(model.Tags))
            content.Add(new StringContent(model.Tags), "Tags");
        if (!string.IsNullOrEmpty(model.VersionComments))
            content.Add(new StringContent(model.VersionComments), "VersionComments");
        content.Add(new StringContent(model.OverwriteExisting.ToString()), "OverwriteExisting");

        var response = await _httpClient.PostAsync("api/documents/upload", content, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<DocumentViewModel>(cancellationToken: cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Downloads a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="versionNumber">The version number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The download result.</returns>
    public async Task<DocumentDownloadResult?> DownloadDocumentAsync(
        Guid id,
        int? versionNumber = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/documents/{id}/download";
        if (versionNumber.HasValue)
            url += $"?versionNumber={versionNumber}";

        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "document";
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

            return new DocumentDownloadResult
            {
                FileContent = content,
                FileName = fileName,
                ContentType = contentType
            };
        }

        return null;
    }

    /// <summary>
    /// Updates a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="model">The edit model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public async Task<bool> UpdateDocumentAsync(
        Guid id,
        EditDocumentViewModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/documents/{id}", model, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Checks out a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="hours">The checkout hours.</param>
    /// <param name="comments">The checkout comments.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public async Task<bool> CheckOutDocumentAsync(
        Guid id,
        int? hours = null,
        string? comments = null,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            DocumentId = id,
            CheckoutHours = hours,
            Comments = comments
        };

        var response = await _httpClient.PostAsJsonAsync($"api/documents/{id}/checkout", request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Checks in a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="updatedFile">The updated file.</param>
    /// <param name="comments">The check-in comments.</param>
    /// <param name="keepCheckedOut">Whether to keep checked out.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public async Task<bool> CheckInDocumentAsync(
        Guid id,
        IFormFile? updatedFile = null,
        string? comments = null,
        bool keepCheckedOut = false,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();

        // Add file if provided
        if (updatedFile != null)
        {
            var fileContent = new StreamContent(updatedFile.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(updatedFile.ContentType);
            content.Add(fileContent, "UpdatedFile", updatedFile.FileName);
        }

        // Add other properties
        content.Add(new StringContent(id.ToString()), "DocumentId");
        if (!string.IsNullOrEmpty(comments))
            content.Add(new StringContent(comments), "Comments");
        content.Add(new StringContent(keepCheckedOut.ToString()), "KeepCheckedOut");

        var response = await _httpClient.PostAsync($"api/documents/{id}/checkin", content, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Deletes a document.
    /// </summary>
    /// <param name="id">The document identifier.</param>
    /// <param name="permanentDelete">Whether to permanently delete.</param>
    /// <param name="reason">The deletion reason.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public async Task<bool> DeleteDocumentAsync(
        Guid id,
        bool permanentDelete = false,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/documents/{id}?permanentDelete={permanentDelete}";
        if (!string.IsNullOrEmpty(reason))
            url += $"&reason={Uri.EscapeDataString(reason)}";

        var response = await _httpClient.DeleteAsync(url, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    #endregion

    #region Search

    /// <summary>
    /// Searches documents.
    /// </summary>
    /// <param name="criteria">The search criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The search results.</returns>
    public async Task<SearchResultsDto?> SearchDocumentsAsync(
        SearchViewModel criteria,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/search/documents", criteria, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SearchResultsDto>(cancellationToken: cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Gets search suggestions.
    /// </summary>
    /// <param name="term">The search term.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The suggestions.</returns>
    public async Task<IEnumerable<string>?> GetSearchSuggestionsAsync(
        string term,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/search/suggestions?term={Uri.EscapeDataString(term)}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<string>>(cancellationToken: cancellationToken);
        }

        return null;
    }

    #endregion

    #region Users

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The users.</returns>
    public async Task<IEnumerable<UserViewModel>?> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/users", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<UserViewModel>>(cancellationToken: cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// Updates a user's role.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="newRole">The new role.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public async Task<bool> UpdateUserRoleAsync(
        Guid userId,
        string newRole,
        CancellationToken cancellationToken = default)
    {
        var request = new { NewRole = newRole };
        var response = await _httpClient.PutAsJsonAsync($"api/users/{userId}/role", request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Activates a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="reason">The activation reason.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public async Task<bool> ActivateUserAsync(
        Guid userId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var request = new { Reason = reason };
        var response = await _httpClient.PostAsJsonAsync($"api/users/{userId}/activate", request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Deactivates a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="reason">The deactivation reason.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public async Task<bool> DeactivateUserAsync(
        Guid userId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var request = new { Reason = reason };
        var response = await _httpClient.PostAsJsonAsync($"api/users/{userId}/deactivate", request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    #endregion

    #region Audit Logs

    /// <summary>
    /// Gets audit logs.
    /// </summary>
    /// <param name="filter">The filter criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The audit log results.</returns>
    public async Task<AuditLogResultsViewModel?> GetAuditLogsAsync(
        AuditLogFilterViewModel filter,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auditlogs", filter, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuditLogResultsViewModel>(cancellationToken: cancellationToken);
        }

        return null;
    }

    #endregion
}





