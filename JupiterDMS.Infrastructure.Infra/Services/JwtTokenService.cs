using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JupiterDMS.Application.Common.Interfaces;
using JupiterDMS.Domain.Constants;
using JupiterDMS.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JupiterDMS.Infrastructure.Infra.Services;

/// <summary>
/// JWT token service implementation for generating and validating JWT tokens.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <exception cref="ArgumentException">Thrown when JWT configuration is missing or invalid.</exception>
    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _secretKey = _configuration[DomainConstants.Jwt.KeyConfigurationKey] 
            ?? throw new ArgumentException("JWT secret key is not configured.");
        
        _issuer = _configuration[DomainConstants.Jwt.IssuerConfigurationKey] 
            ?? throw new ArgumentException("JWT issuer is not configured.");
        
        _audience = _configuration[DomainConstants.Jwt.AudienceConfigurationKey] 
            ?? throw new ArgumentException("JWT audience is not configured.");

        if (!int.TryParse(_configuration[DomainConstants.Jwt.ExpiryMinutesConfigurationKey], out _expiryMinutes))
        {
            _expiryMinutes = DomainConstants.Jwt.DefaultExpiryMinutes;
        }

        if (string.IsNullOrWhiteSpace(_secretKey) || _secretKey.Length < 32)
        {
            throw new ArgumentException("JWT secret key must be at least 32 characters long.");
        }
    }

    /// <inheritdoc/>
    public string GenerateToken(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new(DomainConstants.Jwt.UserIdClaimType, user.Id.ToString()),
            new(DomainConstants.Jwt.UsernameClaimType, user.Username),
            new(DomainConstants.Jwt.RoleClaimType, user.Role.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <inheritdoc/>
    public Guid? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            var userIdClaim = principal.FindFirst(DomainConstants.Jwt.UserIdClaimType);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public string? GetUsernameFromToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            
            return jsonToken.Claims.FirstOrDefault(c => c.Type == DomainConstants.Jwt.UsernameClaimType)?.Value;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public string? GetRoleFromToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            
            return jsonToken.Claims.FirstOrDefault(c => c.Type == DomainConstants.Jwt.RoleClaimType)?.Value;
        }
        catch
        {
            return null;
        }
    }
}
