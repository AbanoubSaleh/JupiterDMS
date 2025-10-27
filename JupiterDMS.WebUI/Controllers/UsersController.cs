using JupiterDMS.WebUI.Models;
using JupiterDMS.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JupiterDMS.WebUI.Controllers;

/// <summary>
/// MVC controller for user management in the UI (Admin only).
/// </summary>
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly JupiterDmsApiClient _apiClient;
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="apiClient">The API client.</param>
    /// <param name="logger">The logger.</param>
    public UsersController(JupiterDmsApiClient apiClient, ILogger<UsersController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Displays the users list.
    /// </summary>
    /// <param name="filter">The filter criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The users view.</returns>
    public async Task<IActionResult> Index(UserFilterViewModel? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            filter ??= new UserFilterViewModel();
            
            var users = await _apiClient.GetUsersAsync(cancellationToken);
            
            if (users != null)
            {
                // Apply client-side filtering (in a real app, this would be done server-side)
                var filteredUsers = users.AsEnumerable();

                if (!filter.IncludeInactive)
                {
                    filteredUsers = filteredUsers.Where(u => u.IsActive);
                }

                if (!string.IsNullOrEmpty(filter.RoleFilter))
                {
                    filteredUsers = filteredUsers.Where(u => u.Role.Equals(filter.RoleFilter, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.ToLowerInvariant();
                    filteredUsers = filteredUsers.Where(u => 
                        u.Username.ToLowerInvariant().Contains(searchTerm) ||
                        u.Email.ToLowerInvariant().Contains(searchTerm) ||
                        u.FullName.ToLowerInvariant().Contains(searchTerm));
                }

                // Apply sorting
                filteredUsers = filter.SortBy.ToLowerInvariant() switch
                {
                    "email" => filter.SortDescending ? filteredUsers.OrderByDescending(u => u.Email) : filteredUsers.OrderBy(u => u.Email),
                    "fullname" => filter.SortDescending ? filteredUsers.OrderByDescending(u => u.FullName) : filteredUsers.OrderBy(u => u.FullName),
                    "role" => filter.SortDescending ? filteredUsers.OrderByDescending(u => u.Role) : filteredUsers.OrderBy(u => u.Role),
                    "createdon" => filter.SortDescending ? filteredUsers.OrderByDescending(u => u.CreatedOn) : filteredUsers.OrderBy(u => u.CreatedOn),
                    "isactive" => filter.SortDescending ? filteredUsers.OrderByDescending(u => u.IsActive) : filteredUsers.OrderBy(u => u.IsActive),
                    _ => filter.SortDescending ? filteredUsers.OrderByDescending(u => u.Username) : filteredUsers.OrderBy(u => u.Username)
                };

                ViewBag.Users = filteredUsers.ToList();
            }
            else
            {
                ViewBag.Users = new List<UserViewModel>();
            }

            ViewBag.Filter = filter;
            ViewBag.Roles = new[] { "Admin", "Editor", "Viewer" };

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            ViewBag.Users = new List<UserViewModel>();
            ViewBag.Filter = filter ?? new UserFilterViewModel();
            ViewBag.Roles = new[] { "Admin", "Editor", "Viewer" };
            return View();
        }
    }

    /// <summary>
    /// Displays the edit user role form.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The edit role view.</returns>
    public async Task<IActionResult> EditRole(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _apiClient.GetUsersAsync(cancellationToken);
            var user = users?.FirstOrDefault(u => u.Id == id);
            
            if (user == null)
            {
                return NotFound("User not found");
            }

            var model = new EditUserRoleViewModel
            {
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                CurrentRole = user.Role,
                NewRole = user.Role
            };

            ViewBag.Roles = new[] { "Admin", "Editor", "Viewer" };
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId} for role editing", id);
            return NotFound();
        }
    }

    /// <summary>
    /// Updates a user's role.
    /// </summary>
    /// <param name="model">The edit role model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect to users list on success; otherwise, the edit view.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(EditUserRoleViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = new[] { "Admin", "Editor", "Viewer" };
            return View(model);
        }

        try
        {
            var success = await _apiClient.UpdateUserRoleAsync(model.UserId, model.NewRole, cancellationToken);

            if (success)
            {
                TempData["SuccessMessage"] = $"User role updated to {model.NewRole} successfully";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Failed to update user role");
            ViewBag.Roles = new[] { "Admin", "Editor", "Viewer" };
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role for user {UserId}", model.UserId);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the user role");
            ViewBag.Roles = new[] { "Admin", "Editor", "Viewer" };
            return View(model);
        }
    }

    /// <summary>
    /// Displays the user activation/deactivation form.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The activation view.</returns>
    public async Task<IActionResult> ToggleActivation(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _apiClient.GetUsersAsync(cancellationToken);
            var user = users?.FirstOrDefault(u => u.Id == id);
            
            if (user == null)
            {
                return NotFound("User not found");
            }

            var model = new UserActivationViewModel
            {
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                IsActive = user.IsActive
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId} for activation toggle", id);
            return NotFound();
        }
    }

    /// <summary>
    /// Toggles user activation status.
    /// </summary>
    /// <param name="model">The activation model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Redirect to users list on success; otherwise, the activation view.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActivation(UserActivationViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            bool success;
            string action;

            if (model.IsActive)
            {
                // Deactivate user
                success = await _apiClient.DeactivateUserAsync(model.UserId, model.Reason, cancellationToken);
                action = "deactivated";
            }
            else
            {
                // Activate user
                success = await _apiClient.ActivateUserAsync(model.UserId, model.Reason, cancellationToken);
                action = "activated";
            }

            if (success)
            {
                TempData["SuccessMessage"] = $"User {action} successfully";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, $"Failed to {action.TrimEnd('d')} user");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling activation for user {UserId}", model.UserId);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the user status");
            return View(model);
        }
    }
}
