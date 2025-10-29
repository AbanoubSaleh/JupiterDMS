using JupiterDMS.Application.Common.Interfaces;
using JupiterDMS.Domain.Constants;
using JupiterDMS.Domain.Entities;
using JupiterDMS.Domain.Enums;

namespace JupiterDMS.Application.Services;

/// <summary>
/// Service implementation for user management operations.
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="authService">The authentication service.</param>
    public UserService(IUnitOfWork unitOfWork, IUserRepository userRepository, IAuthService authService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<User>> GetAllUsersAsync(bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        
        if (!includeDeleted)
        {
            users = users.Where(u => !u.IsDeleted);
        }

        return users.OrderBy(u => u.Username);
    }

    /// <inheritdoc/>
    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByUsernameAsync(username, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByEmailAsync(email, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<User> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        if (password.Length < DomainConstants.Auth.MinPasswordLength)
            throw new ArgumentException($"Password must be at least {DomainConstants.Auth.MinPasswordLength} characters long.", nameof(password));

        if (user.Username.Length < DomainConstants.Auth.MinUsernameLength)
            throw new ArgumentException($"Username must be at least {DomainConstants.Auth.MinUsernameLength} characters long.", nameof(user.Username));

        // Check if username or email already exists
        if (await _userRepository.UsernameExistsAsync(user.Username, cancellationToken))
            throw new InvalidOperationException($"Username '{user.Username}' already exists.");

        if (await _userRepository.EmailExistsAsync(user.Email, cancellationToken))
            throw new InvalidOperationException($"Email '{user.Email}' already exists.");

        // Hash the password
        user.PasswordHash = _authService.HashPassword(password);
        user.Id = Guid.NewGuid();
        user.CreatedOn = DateTime.UtcNow;
        user.CreatedBy = user.Email; // Self-created for new users
        user.IsActive = true;
        user.IsDeleted = false;

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return user;
    }

    /// <inheritdoc/>
    public async Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var existingUser = await _userRepository.GetByIdAsync(user.Id, cancellationToken);
        if (existingUser == null)
            throw new InvalidOperationException($"User with ID '{user.Id}' not found.");

        // Check if username or email already exists (excluding current user)
        var usernameExists = await _userRepository.UsernameExistsAsync(user.Username, cancellationToken);
        if (usernameExists && existingUser.Username != user.Username)
            throw new InvalidOperationException($"Username '{user.Username}' already exists.");

        var emailExists = await _userRepository.EmailExistsAsync(user.Email, cancellationToken);
        if (emailExists && existingUser.Email != user.Email)
            throw new InvalidOperationException($"Email '{user.Email}' already exists.");

        // Update properties
        existingUser.Username = user.Username;
        existingUser.Email = user.Email;
        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Role = user.Role;
        existingUser.IsActive = user.IsActive;
        existingUser.ModifiedOn = DateTime.UtcNow;
        existingUser.ModifiedBy = user.ModifiedBy;

        await _userRepository.UpdateAsync(existingUser, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return existingUser;
    }

    /// <inheritdoc/>
    public async Task<bool> ChangePasswordAsync(string userEmail, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            return false;

        if (newPassword.Length < DomainConstants.Auth.MinPasswordLength)
            return false;

        var user = await GetUserByEmailAsync(userEmail, cancellationToken);
        if (user == null || !user.IsActive)
            return false;

        // Verify current password
        if (!_authService.VerifyPassword(currentPassword, user.PasswordHash))
            return false;

        // Update password
        user.PasswordHash = _authService.HashPassword(newPassword);
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = userEmail;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> ResetPasswordAsync(string userEmail, string newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            return false;

        if (newPassword.Length < DomainConstants.Auth.MinPasswordLength)
            return false;

        var user = await GetUserByEmailAsync(userEmail, cancellationToken);
        if (user == null)
            return false;

        // Update password
        user.PasswordHash = _authService.HashPassword(newPassword);
        user.ModifiedOn = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> ChangeUserRoleAsync(Guid userId, UserRole newRole, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return false;

        user.Role = newRole;
        user.ModifiedOn = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> SetUserActiveStatusAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return false;

        user.IsActive = isActive;
        user.ModifiedOn = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            return false;

        user.IsDeleted = true;
        user.IsActive = false;
        user.ModifiedOn = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        var exists = await _userRepository.UsernameExistsAsync(username, cancellationToken);
        
        if (exists && excludeUserId.HasValue)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(username, cancellationToken);
            return existingUser?.Id != excludeUserId.Value;
        }

        return exists;
    }

    /// <inheritdoc/>
    public async Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var exists = await _userRepository.EmailExistsAsync(email, cancellationToken);
        
        if (exists && excludeUserId.HasValue)
        {
            var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
            return existingUser?.Id != excludeUserId.Value;
        }

        return exists;
    }

    /// <inheritdoc/>
    public async Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return;

        user.LastLoginOn = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}
