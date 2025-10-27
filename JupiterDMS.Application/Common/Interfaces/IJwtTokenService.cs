using JupiterDMS.Domain.Entities;

namespace JupiterDMS.Application.Common.Interfaces;

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom to generate the token.</param>
    /// <returns>The generated JWT token.</returns>
    string GenerateToken(User user);

    /// <summary>
    /// Validates a JWT token and returns the user ID if valid.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>The user ID if the token is valid; otherwise, null.</returns>
    Guid? ValidateToken(string token);

    /// <summary>
    /// Extracts the username from a JWT token.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns>The username if the token is valid; otherwise, null.</returns>
    string? GetUsernameFromToken(string token);

    /// <summary>
    /// Extracts the role from a JWT token.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns>The role if the token is valid; otherwise, null.</returns>
    string? GetRoleFromToken(string token);
}
