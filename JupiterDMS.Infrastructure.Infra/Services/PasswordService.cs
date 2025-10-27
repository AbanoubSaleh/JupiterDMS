using JupiterDMS.Application.Common.Interfaces;
using BCrypt.Net;

namespace JupiterDMS.Infrastructure.Infra.Services;

/// <summary>
/// Password service implementation using BCrypt for hashing and verification.
/// </summary>
public class PasswordService : IPasswordService
{
    /// <inheritdoc/>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <inheritdoc/>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
