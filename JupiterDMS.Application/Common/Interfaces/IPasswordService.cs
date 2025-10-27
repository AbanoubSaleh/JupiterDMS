namespace JupiterDMS.Application.Common.Interfaces;

/// <summary>
/// Service for password hashing and verification.
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hashes a password using a secure hashing algorithm.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="hash">The hash to verify against.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    bool VerifyPassword(string password, string hash);
}
