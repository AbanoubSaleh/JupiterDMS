using JupiterDMS.Domain.Enums;

namespace JupiterDMS.Application.Common.Interfaces;

/// <summary>
/// Service for accessing current user context.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user identifier.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the current username.
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Gets the current user's email.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the current user's role.
    /// </summary>
    UserRole? Role { get; }

    /// <summary>
    /// Gets whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets whether the current user is an admin.
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// Gets whether the current user is an editor or higher.
    /// </summary>
    bool IsEditor { get; }

    /// <summary>
    /// Gets the current user's IP address.
    /// </summary>
    string? IpAddress { get; }

    /// <summary>
    /// Gets the current user's user agent.
    /// </summary>
    string? UserAgent { get; }
}
