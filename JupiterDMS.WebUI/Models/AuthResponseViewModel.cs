namespace JupiterDMS.WebUI.Models;

/// <summary>
/// Simple API response model.
/// </summary>
public class AuthResponseViewModel
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}
