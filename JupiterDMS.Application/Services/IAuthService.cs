using JupiterDMS.Domain.Entities;

namespace JupiterDMS.Application.Services;

/// <summary>
/// Service interface for authentication operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with username and password.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The authenticated user if successful; otherwise, null.</returns>
    Task<User?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate token for.</param>
    /// <returns>The JWT token.</returns>
    string GenerateJwtToken(User user);

    /// <summary>
    /// Hashes a password using a secure hashing algorithm.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against its hash.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="hash">The hash to verify against.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Validates a JWT token and returns the user ID.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>The user ID if the token is valid; otherwise, null.</returns>
    Guid? ValidateToken(string token);

    /// <summary>
    /// Refreshes a JWT token.
    /// </summary>
    /// <param name="token">The token to refresh.</param>
    /// <returns>A new JWT token if the original is valid; otherwise, null.</returns>
    string? RefreshToken(string token);
}
