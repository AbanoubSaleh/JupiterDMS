namespace JupiterDMS.Domain.Enums;

/// <summary>
/// Defines the types of actions that can be audited in the system.
/// </summary>
public enum AuditActionType
{
    /// <summary>
    /// Entity was created.
    /// </summary>
    Create = 0,

    /// <summary>
    /// Entity was updated.
    /// </summary>
    Update = 1,

    /// <summary>
    /// Entity was deleted.
    /// </summary>
    Delete = 2,

    /// <summary>
    /// Entity was viewed or accessed.
    /// </summary>
    View = 3,

    /// <summary>
    /// Document was downloaded.
    /// </summary>
    Download = 4,

    /// <summary>
    /// Document was uploaded.
    /// </summary>
    Upload = 5,

    /// <summary>
    /// User logged in.
    /// </summary>
    Login = 6,

    /// <summary>
    /// User logged out.
    /// </summary>
    Logout = 7,

    /// <summary>
    /// Permission was granted.
    /// </summary>
    PermissionGranted = 8,

    /// <summary>
    /// Permission was revoked.
    /// </summary>
    PermissionRevoked = 9,

    /// <summary>
    /// Document was checked out for editing.
    /// </summary>
    CheckOut = 10,

    /// <summary>
    /// Document was checked in after editing.
    /// </summary>
    CheckIn = 11,

    /// <summary>
    /// Document was moved to a different location.
    /// </summary>
    Move = 12,

    /// <summary>
    /// Document was copied.
    /// </summary>
    Copy = 13,

    /// <summary>
    /// Document was renamed.
    /// </summary>
    Rename = 14,

    /// <summary>
    /// Document properties were updated.
    /// </summary>
    PropertiesUpdate = 15,

    /// <summary>
    /// Document version was restored.
    /// </summary>
    VersionRestore = 16,

    /// <summary>
    /// Search operation was performed.
    /// </summary>
    Search = 17,

    /// <summary>
    /// User role was changed.
    /// </summary>
    RoleChange = 18,

    /// <summary>
    /// User was activated.
    /// </summary>
    UserActivated = 19,

    /// <summary>
    /// User was deactivated.
    /// </summary>
    UserDeactivated = 20
}

