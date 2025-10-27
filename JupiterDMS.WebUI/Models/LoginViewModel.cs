using System.ComponentModel.DataAnnotations;

namespace JupiterDMS.WebUI.Models;

/// <summary>
/// Simple login model.
/// </summary>
public class LoginViewModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
