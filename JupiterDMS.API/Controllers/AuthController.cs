using AutoMapper;
using JupiterDMS.Application.Common.DTOs;
using JupiterDMS.Application.Services;
using JupiterDMS.Domain.Constants;
using JupiterDMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JupiterDMS.API.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    /// <param name="userService">The user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger.</param>
    public AuthController(
        IAuthService authService,
        IUserService userService,
        IMapper mapper,
        ILogger<AuthController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The login response with JWT token.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> LoginAsync(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _authService.AuthenticateAsync(request.Username, request.Password, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
                return Unauthorized("Invalid username or password.");
            }

            var token = _authService.GenerateJwtToken(user);
            var userDto = _mapper.Map<UserDto>(user);

            // Update last login time
            await _userService.UpdateLastLoginAsync(user.Id, cancellationToken);

            var response = new LoginResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(DomainConstants.Jwt.DefaultExpiryMinutes),
                User = userDto
            };

            _logger.LogInformation("User {Username} logged in successfully", user.Username);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
            return StatusCode(500, "An error occurred during login.");
        }
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created user information.</returns>
    /// <response code="201">User created successfully.</response>
    /// <response code="400">Invalid request or user already exists.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> RegisterAsync(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if username or email already exists
            if (await _userService.UsernameExistsAsync(request.Username, cancellationToken: cancellationToken))
            {
                return BadRequest($"Username '{request.Username}' already exists.");
            }

            if (await _userService.EmailExistsAsync(request.Email, cancellationToken: cancellationToken))
            {
                return BadRequest($"Email '{request.Email}' already exists.");
            }

            var user = _mapper.Map<User>(request);
            var createdUser = await _userService.CreateUserAsync(user, request.Password, cancellationToken);
            var userDto = _mapper.Map<UserDto>(createdUser);

            _logger.LogInformation("User {Username} registered successfully", createdUser.Username);
            return CreatedAtAction(nameof(GetCurrentUser), new { id = createdUser.Id }, userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for username: {Username}", request.Username);
            return StatusCode(500, "An error occurred during registration.");
        }
    }

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    /// <param name="request">The password change request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="200">Password changed successfully.</response>
    /// <response code="400">Invalid request or current password is incorrect.</response>
    /// <response code="401">Unauthorized.</response>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePasswordAsync(
        [FromBody] ChangePasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(DomainConstants.Jwt.UserIdClaimType);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var success = await _userService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword, cancellationToken);
            if (!success)
            {
                return BadRequest("Current password is incorrect or new password is invalid.");
            }

            _logger.LogInformation("User {UserId} changed password successfully", userId);
            return Ok("Password changed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change");
            return StatusCode(500, "An error occurred while changing password.");
        }
    }

    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current user information.</returns>
    /// <response code="200">User information retrieved successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(DomainConstants.Jwt.UserIdClaimType);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user information");
            return StatusCode(500, "An error occurred while retrieving user information.");
        }
    }

    /// <summary>
    /// Validates a JWT token.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>Token validation result.</returns>
    /// <response code="200">Token is valid.</response>
    /// <response code="400">Token is invalid.</response>
    [HttpPost("validate-token")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ValidateToken([FromBody] string token)
    {
        try
        {
            var userId = _authService.ValidateToken(token);
            if (userId == null)
            {
                return BadRequest("Invalid token.");
            }

            return Ok(new { Valid = true, UserId = userId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return BadRequest("Invalid token.");
        }
    }
}
