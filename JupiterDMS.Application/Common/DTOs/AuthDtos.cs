using System.ComponentModel.DataAnnotations;

namespace JupiterDMS.Application.Common.DTOs;

/// <summary>
/// Data transfer object for login requests.
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters.")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for login responses.
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// Gets or sets the JWT token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token expiry date.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the user information.
    /// </summary>
    public UserDto User { get; set; } = null!;
}

/// <summary>
/// Data transfer object for user registration.
/// </summary>
public class RegisterRequestDto
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters.")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
    public string LastName { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for password change requests.
/// </summary>
public class ChangePasswordRequestDto
{
    /// <summary>
    /// Gets or sets the current password.
    /// </summary>
    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new password.
    /// </summary>
    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be between 6 and 100 characters.")]
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for password reset requests.
/// </summary>
public class ResetPasswordRequestDto
{
    /// <summary>
    /// Gets or sets the new password.
    /// </summary>
    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "New password must be between 6 and 100 characters.")]
    public string NewPassword { get; set; } = string.Empty;
}
