using System.Net;
using System.Text.Json;
using FluentValidation;
using JupiterDMS.Application.Common.DTOs;
using JupiterDMS.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace JupiterDMS.API.Middleware;

/// <summary>
/// Middleware for handling exceptions globally.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware delegate.</param>
    /// <param name="logger">The logger.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        ErrorResponseDto response;

        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                var validationErrors = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                response = ErrorResponseDto.Create("Validation failed", validationErrors);
                response.Code = "VALIDATION_ERROR";
                break;

            case EntityNotFoundException notFoundEx:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response = ErrorResponseDto.Create(notFoundEx.Message, "ENTITY_NOT_FOUND");
                break;

            case DomainException domainEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = ErrorResponseDto.Create(domainEx.Message, "DOMAIN_ERROR");
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response = ErrorResponseDto.Create("An internal server error occurred", "INTERNAL_ERROR");
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }


}

