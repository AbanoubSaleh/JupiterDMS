using JupiterDMS.Application.Common.Interfaces;
using JupiterDMS.Infrastructure.DataAccess.Persistence;
using JupiterDMS.Infrastructure.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JupiterDMS.Infrastructure.DataAccess;

/// <summary>
/// Dependency injection configuration for the DataAccess layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Configuration key for the database connection string.
    /// </summary>
    public const string ConnectionStringKey = "DefaultConnection";

    /// <summary>
    /// Fallback configuration key for the database connection string.
    /// </summary>
    public const string FallbackConnectionStringKey = "JupiterDmsConnection";

    /// <summary>
    /// Adds DataAccess layer services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringKey)
            ?? configuration.GetConnectionString(FallbackConnectionStringKey);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringKey}' or '{FallbackConnectionStringKey}' not found in configuration.");
        }

        // Register DbContext with SQL Server
        services.AddDbContext<JupiterDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register User Repository
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}

