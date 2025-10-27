using System.ComponentModel.DataAnnotations;

namespace JupiterDMS.WebUI.Models;

/// <summary>
/// Simple registration model.
/// </summary>
public class RegisterViewModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string Role { get; set; } = "Viewer";
}
