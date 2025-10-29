using JupiterDMS.Domain.Entities;
using JupiterDMS.Domain.Enums;

namespace JupiterDMS.Application.Services;

/// <summary>
/// Service interface for user management operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <param name="includeDeleted">Whether to include deleted users.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of users.</returns>
    Task<IEnumerable<User>> GetAllUsersAsync(bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by username.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created user.</returns>
    Task<User> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated user.</returns>
    Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="currentPassword">The current password.</param>
    /// <param name="newPassword">The new password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the password was changed successfully; otherwise, false.</returns>
    Task<bool> ChangePasswordAsync(string userEmail, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets a user's password (admin only).
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="newPassword">The new password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the password was reset successfully; otherwise, false.</returns>
    Task<bool> ResetPasswordAsync(string userEmail, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes a user's role.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="newRole">The new role.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the role was changed successfully; otherwise, false.</returns>
    Task<bool> ChangeUserRoleAsync(Guid userId, UserRole newRole, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates or deactivates a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="isActive">Whether the user should be active.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the status was changed successfully; otherwise, false.</returns>
    Task<bool> SetUserActiveStatusAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user (soft delete).
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted successfully; otherwise, false.</returns>
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a username already exists.
    /// </summary>
    /// <param name="username">The username to check.</param>
    /// <param name="excludeUserId">User ID to exclude from the check (for updates).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the username exists; otherwise, false.</returns>
    Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email already exists.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <param name="excludeUserId">User ID to exclude from the check (for updates).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the email exists; otherwise, false.</returns>
    Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the last login time for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default);
}
