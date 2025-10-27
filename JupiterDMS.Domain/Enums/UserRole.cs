namespace JupiterDMS.Domain.Enums;

/// <summary>
/// Defines the roles available for users in the system.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Guest user with read-only access.
    /// </summary>
    Guest = 0,

    /// <summary>
    /// Regular user with standard permissions.
    /// </summary>
    User = 1,

    /// <summary>
    /// Power user with extended permissions.
    /// </summary>
    PowerUser = 2,

    /// <summary>
    /// Administrator with full system access.
    /// </summary>
    Administrator = 3,

    /// <summary>
    /// System administrator with unrestricted access.
    /// </summary>
    SystemAdministrator = 4,

    /// <summary>
    /// Viewer role with read-only access to documents.
    /// </summary>
    Viewer = 10,

    /// <summary>
    /// Editor role with ability to create and modify documents.
    /// </summary>
    Editor = 11,

    /// <summary>
    /// Admin role with full administrative access.
    /// </summary>
    Admin = 12
}

