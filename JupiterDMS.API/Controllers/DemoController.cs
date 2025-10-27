using JupiterDMS.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JupiterDMS.API.Controllers;

/// <summary>
/// Demo controller for testing role-based authorization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class DemoController : ControllerBase
{
    private readonly ILogger<DemoController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DemoController"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public DemoController(ILogger<DemoController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Demo endpoint accessible only by Admin role.
    /// </summary>
    /// <returns>A demo response for admin users.</returns>
    /// <response code="200">Success - Admin access granted.</response>
    /// <response code="401">Unauthorized - No valid JWT token.</response>
    /// <response code="403">Forbidden - User does not have Admin role.</response>
    [HttpGet("admin")]
    [Authorize(Policy = DomainConstants.Auth.AdminPolicy)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetAdminData()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = User.FindFirst(DomainConstants.Jwt.UserIdClaimType)?.Value;

        _logger.LogInformation("Admin endpoint accessed by user: {Username} with role: {Role}", username, role);

        return Ok(new
        {
            Message = "Welcome to the Admin area! You have full administrative access.",
            AccessLevel = "Admin",
            Username = username,
            Role = role,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            AvailableActions = new[]
            {
                "Create users",
                "Delete users",
                "Manage system settings",
                "View all documents",
                "Manage libraries",
                "System administration"
            }
        });
    }

    /// <summary>
    /// Demo endpoint accessible by Admin and Editor roles.
    /// </summary>
    /// <returns>A demo response for editor users.</returns>
    /// <response code="200">Success - Editor access granted.</response>
    /// <response code="401">Unauthorized - No valid JWT token.</response>
    /// <response code="403">Forbidden - User does not have Editor or Admin role.</response>
    [HttpGet("editor")]
    [Authorize(Policy = DomainConstants.Auth.EditorPolicy)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetEditorData()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = User.FindFirst(DomainConstants.Jwt.UserIdClaimType)?.Value;

        _logger.LogInformation("Editor endpoint accessed by user: {Username} with role: {Role}", username, role);

        return Ok(new
        {
            Message = "Welcome to the Editor area! You can create and modify documents.",
            AccessLevel = "Editor",
            Username = username,
            Role = role,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            AvailableActions = new[]
            {
                "Create documents",
                "Edit documents",
                "Upload files",
                "Manage folders",
                "View documents",
                "Download documents"
            }
        });
    }

    /// <summary>
    /// Demo endpoint accessible by all authenticated users (Admin, Editor, and Viewer roles).
    /// </summary>
    /// <returns>A demo response for viewer users.</returns>
    /// <response code="200">Success - Viewer access granted.</response>
    /// <response code="401">Unauthorized - No valid JWT token.</response>
    [HttpGet("viewer")]
    [Authorize(Policy = DomainConstants.Auth.ViewerPolicy)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetViewerData()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = User.FindFirst(DomainConstants.Jwt.UserIdClaimType)?.Value;

        _logger.LogInformation("Viewer endpoint accessed by user: {Username} with role: {Role}", username, role);

        return Ok(new
        {
            Message = "Welcome to the Viewer area! You have read-only access to documents.",
            AccessLevel = "Viewer",
            Username = username,
            Role = role,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            AvailableActions = new[]
            {
                "View documents",
                "Download documents",
                "Search documents",
                "Browse libraries",
                "View document history"
            }
        });
    }

    /// <summary>
    /// Demo endpoint to get current user information.
    /// </summary>
    /// <returns>Current user information from JWT token.</returns>
    /// <response code="200">Success - User information retrieved.</response>
    /// <response code="401">Unauthorized - No valid JWT token.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = User.FindFirst(DomainConstants.Jwt.UserIdClaimType)?.Value;

        _logger.LogInformation("User info endpoint accessed by user: {Username}", username);

        return Ok(new
        {
            Message = "Current user information",
            Username = username,
            Role = role,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToArray()
        });
    }
}
