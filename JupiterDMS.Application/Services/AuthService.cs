using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JupiterDMS.Application.Common.Interfaces;
using JupiterDMS.Domain.Constants;
using JupiterDMS.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JupiterDMS.Application.Services;

/// <summary>
/// Service implementation for authentication operations.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="configuration">The configuration.</param>
    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc/>
    public async Task<User?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        if (user == null || !user.IsActive)
            return null;

        if (!VerifyPassword(password, user.PasswordHash))
            return null;

        return user;
    }

    /// <inheritdoc/>
    public string GenerateJwtToken(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var jwtKey = _configuration[DomainConstants.Jwt.KeyConfigurationKey];
        var jwtIssuer = _configuration[DomainConstants.Jwt.IssuerConfigurationKey];
        var jwtAudience = _configuration[DomainConstants.Jwt.AudienceConfigurationKey];
        var expiryMinutes = _configuration.GetValue<int>(DomainConstants.Jwt.ExpiryMinutesConfigurationKey, DomainConstants.Jwt.DefaultExpiryMinutes);

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            throw new InvalidOperationException("JWT configuration is missing.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(DomainConstants.Jwt.UserIdClaimType, user.Id.ToString()),
            new Claim(DomainConstants.Jwt.UsernameClaimType, user.Username),
            new Claim(DomainConstants.Jwt.RoleClaimType, user.Role.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, GetRoleName(user.Role)),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc/>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        // Using BCrypt for password hashing
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    /// <inheritdoc/>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
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

    /// <inheritdoc/>
    public Guid? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var jwtKey = _configuration[DomainConstants.Jwt.KeyConfigurationKey];
            var jwtIssuer = _configuration[DomainConstants.Jwt.IssuerConfigurationKey];
            var jwtAudience = _configuration[DomainConstants.Jwt.AudienceConfigurationKey];

            if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            var userIdClaim = principal.FindFirst(DomainConstants.Jwt.UserIdClaimType);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                return userId;

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public string? RefreshToken(string token)
    {
        var userId = ValidateToken(token);
        if (userId == null)
            return null;

        // In a real implementation, you might want to check if the user is still active
        // and generate a new token with updated claims
        // For now, we'll return null to indicate refresh is not supported
        return null;
    }

    /// <summary>
    /// Gets the role name for JWT claims.
    /// </summary>
    /// <param name="role">The user role.</param>
    /// <returns>The role name.</returns>
    private static string GetRoleName(Domain.Enums.UserRole role)
    {
        return role switch
        {
            Domain.Enums.UserRole.Admin => DomainConstants.Auth.AdminRole,
            Domain.Enums.UserRole.Editor => DomainConstants.Auth.EditorRole,
            Domain.Enums.UserRole.Viewer => DomainConstants.Auth.ViewerRole,
            _ => DomainConstants.Auth.ViewerRole
        };
    }
}
