using System.ComponentModel.DataAnnotations;

namespace JupiterDMS.Application.Common.DTOs;

/// <summary>
/// Standardized error response model for API endpoints.
/// </summary>
public class ErrorResponseDto
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation errors (if applicable).
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Gets or sets the error code (optional).
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Gets or sets additional details about the error (optional).
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the error occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a simple error response with just a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>An error response DTO.</returns>
    public static ErrorResponseDto Create(string message)
    {
        return new ErrorResponseDto { Message = message };
    }

    /// <summary>
    /// Creates an error response with a message and error code.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <returns>An error response DTO.</returns>
    public static ErrorResponseDto Create(string message, string code)
    {
        return new ErrorResponseDto { Message = message, Code = code };
    }

    /// <summary>
    /// Creates an error response with validation errors.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errors">The validation errors.</param>
    /// <returns>An error response DTO.</returns>
    public static ErrorResponseDto Create(string message, Dictionary<string, string[]> errors)
    {
        return new ErrorResponseDto { Message = message, Errors = errors };
    }
}
