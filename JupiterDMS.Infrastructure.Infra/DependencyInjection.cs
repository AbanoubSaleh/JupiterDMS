using JupiterDMS.Application.Common.Interfaces;
using JupiterDMS.Infrastructure.Infra.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JupiterDMS.Infrastructure.Infra;

/// <summary>
/// Dependency injection configuration for the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfra(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register file storage service
        services.AddScoped<IFileStorageService, FileStorageService>();

        // Register JWT token service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Register password service
        services.AddScoped<IPasswordService, PasswordService>();

        // Register audit log service
        services.AddScoped<IAuditLogService, AuditLogService>();

        // Register current user service
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register HttpContextAccessor for CurrentUserService
        services.AddHttpContextAccessor();

        // Register HttpClient factory
        services.AddHttpClient();

        return services;
    }
}

