using AutoMapper;
using JupiterDMS.Application.Common.DTOs;
using JupiterDMS.Application.Services;
using JupiterDMS.Domain.Constants;
using JupiterDMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JupiterDMS.API.Controllers;

/// <summary>
/// Controller for user management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="userService">The user service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger.</param>
    public UsersController(
        IUserService userService,
        IMapper mapper,
        ILogger<UsersController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <param name="includeDeleted">Whether to include deleted users.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of users.</returns>
    /// <response code="200">Users retrieved successfully.</response>
    /// <response code="403">Forbidden - Admin access required.</response>
    [HttpGet]
    [Authorize(Policy = DomainConstants.Auth.AdminPolicy)]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsersAsync(
        [FromQuery] bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userService.GetAllUsersAsync(includeDeleted, cancellationToken);
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, "An error occurred while retrieving users.");
        }
    }

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user information.</returns>
    /// <response code="200">User retrieved successfully.</response>
    /// <response code="404">User not found.</response>
    /// <response code="403">Forbidden - Admin access required.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = DomainConstants.Auth.AdminPolicy)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDto>> GetUserByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return NotFound($"User with ID '{id}' not found.");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, "An error occurred while retrieving the user.");
        }
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="request">The user creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created user information.</returns>
    /// <response code="201">User created successfully.</response>
    /// <response code="400">Invalid request or user already exists.</response>
    /// <response code="403">Forbidden - Admin access required.</response>
    [HttpPost]
    [Authorize(Policy = DomainConstants.Auth.AdminPolicy)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDto>> CreateUserAsync(
        [FromBody] CreateUserDto request,
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
            
            // Set created by from current user
            var userIdClaim = User.FindFirst(DomainConstants.Jwt.UserIdClaimType);
            Guid? currentUserId = null;
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                currentUserId = parsedUserId;
                user.CreatedBy = parsedUserId;
            }

            var createdUser = await _userService.CreateUserAsync(user, request.Password, cancellationToken);
            var userDto = _mapper.Map<UserDto>(createdUser);

            _logger.LogInformation("User {Username} created successfully by {CurrentUserId}", createdUser.Username, currentUserId);
            return CreatedAtAction(nameof(GetUserByIdAsync), new { id = createdUser.Id }, userDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Username}", request.Username);
            return StatusCode(500, "An error occurred while creating the user.");
        }
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="request">The user update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated user information.</returns>
    /// <response code="200">User updated successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">User not found.</response>
    /// <response code="403">Forbidden - Admin access required.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = DomainConstants.Auth.AdminPolicy)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDto>> UpdateUserAsync(
        Guid id,
        [FromBody] UpdateUserDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != request.Id)
            {
                return BadRequest("ID in URL does not match ID in request body.");
            }

            var user = _mapper.Map<User>(request);
            
            // Set modified by from current user
            var userIdClaim = User.FindFirst(DomainConstants.Jwt.UserIdClaimType);
            Guid? currentUserId = null;
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                currentUserId = parsedUserId;
                user.ModifiedBy = parsedUserId;
            }

            var updatedUser = await _userService.UpdateUserAsync(user, cancellationToken);
            var userDto = _mapper.Map<UserDto>(updatedUser);

            _logger.LogInformation("User {UserId} updated successfully by {CurrentUserId}", id, currentUserId);
            return Ok(userDto);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, "An error occurred while updating the user.");
        }
    }

    /// <summary>
    /// Changes a user's role.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="request">The role change request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="200">Role changed successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">User not found.</response>
    /// <response code="403">Forbidden - Admin access required.</response>
    [HttpPatch("{id:guid}/role")]
    [Authorize(Policy = DomainConstants.Auth.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangeUserRoleAsync(
        Guid id,
        [FromBody] ChangeUserRoleDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _userService.ChangeUserRoleAsync(id, request.Role, cancellationToken);
            if (!success)
            {
                return NotFound($"User with ID '{id}' not found.");
            }

            _logger.LogInformation("User {UserId} role changed to {Role}", id, request.Role);
            return Ok("User role changed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing role for user {UserId}", id);
            return StatusCode(500, "An error occurred while changing the user role.");
        }
    }

    /// <summary>
    /// Sets a user's active status.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="request">The active status request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="200">Status changed successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">User not found.</response>
    /// <response code="403">Forbidden - Admin access required.</response>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = DomainConstants.Auth.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetUserActiveStatusAsync(
        Guid id,
        [FromBody] SetUserActiveStatusDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _userService.SetUserActiveStatusAsync(id, request.IsActive, cancellationToken);
            if (!success)
            {
                return NotFound($"User with ID '{id}' not found.");
            }

            var status = request.IsActive ? "activated" : "deactivated";
            _logger.LogInformation("User {UserId} {Status}", id, status);
            return Ok($"User {status} successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing status for user {UserId}", id);
            return StatusCode(500, "An error occurred while changing the user status.");
        }
    }

    /// <summary>
    /// Resets a user's password.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="request">The password reset request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="200">Password reset successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">User not found.</response>
    /// <response code="403">Forbidden - Admin access required.</response>
    [HttpPost("{id:guid}/reset-password")]
    [Authorize(Policy = DomainConstants.Auth.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResetPasswordAsync(
        Guid id,
        [FromBody] ResetPasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _userService.ResetPasswordAsync(id, request.NewPassword, cancellationToken);
            if (!success)
            {
                return NotFound($"User with ID '{id}' not found.");
            }

            _logger.LogInformation("Password reset for user {UserId}", id);
            return Ok("Password reset successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user {UserId}", id);
            return StatusCode(500, "An error occurred while resetting the password.");
        }
    }

    /// <summary>
    /// Deletes a user (soft delete).
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="200">User deleted successfully.</response>
    /// <response code="404">User not found.</response>
    /// <response code="403">Forbidden - Admin access required.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = DomainConstants.Auth.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUserAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _userService.DeleteUserAsync(id, cancellationToken);
            if (!success)
            {
                return NotFound($"User with ID '{id}' not found.");
            }

            _logger.LogInformation("User {UserId} deleted successfully", id);
            return Ok("User deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, "An error occurred while deleting the user.");
        }
    }
}
