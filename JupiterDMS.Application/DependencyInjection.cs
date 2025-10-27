using System.Reflection;
using FluentValidation;
using JupiterDMS.Application.Common.Behaviors;
using JupiterDMS.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace JupiterDMS.Application;

/// <summary>
/// Dependency injection configuration for the Application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application layer services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register Application Services
        services.AddScoped<ILibraryService, LibraryService>();

        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Register AutoMapper with explicit configuration
        services.AddAutoMapper(cfg => cfg.AddMaps(assembly));

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        // Register pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}

