using System.ComponentModel.DataAnnotations;

namespace JupiterDMS.WebUI.Models;

/// <summary>
/// View model for user display.
/// </summary>
public class UserViewModel
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full name.
    /// </summary>
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user role.
    /// </summary>
    [Display(Name = "Role")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the user is active.
    /// </summary>
    [Display(Name = "Active")]
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets when the user was created.
    /// </summary>
    [Display(Name = "Created On")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets who created the user.
    /// </summary>
    [Display(Name = "Created By")]
    public string? CreatedByUsername { get; set; }

    /// <summary>
    /// Gets or sets when the user was last modified.
    /// </summary>
    [Display(Name = "Modified On")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets who last modified the user.
    /// </summary>
    [Display(Name = "Modified By")]
    public string? ModifiedByUsername { get; set; }

    /// <summary>
    /// Gets or sets when the user last logged in.
    /// </summary>
    [Display(Name = "Last Login")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
    public DateTime? LastLoginOn { get; set; }

    /// <summary>
    /// Gets the status display text.
    /// </summary>
    [Display(Name = "Status")]
    public string StatusText => IsActive ? "Active" : "Inactive";

    /// <summary>
    /// Gets the CSS class for status display.
    /// </summary>
    public string StatusCssClass => IsActive ? "badge bg-success" : "badge bg-secondary";
}

/// <summary>
/// View model for editing user role.
/// </summary>
public class EditUserRoleViewModel
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the username for display.
    /// </summary>
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full name for display.
    /// </summary>
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current role.
    /// </summary>
    [Display(Name = "Current Role")]
    public string CurrentRole { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new role.
    /// </summary>
    [Required(ErrorMessage = "Please select a role")]
    [Display(Name = "New Role")]
    public string NewRole { get; set; } = string.Empty;
}

/// <summary>
/// View model for user activation/deactivation.
/// </summary>
public class UserActivationViewModel
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the username for display.
    /// </summary>
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full name for display.
    /// </summary>
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status.
    /// </summary>
    [Display(Name = "Current Status")]
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the reason for activation/deactivation.
    /// </summary>
    [Required(ErrorMessage = "Please provide a reason")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    [Display(Name = "Reason")]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets the action text.
    /// </summary>
    public string ActionText => IsActive ? "Deactivate" : "Activate";

    /// <summary>
    /// Gets the confirmation text.
    /// </summary>
    public string ConfirmationText => IsActive 
        ? $"Are you sure you want to deactivate user '{Username}'?" 
        : $"Are you sure you want to activate user '{Username}'?";
}

/// <summary>
/// View model for user management filters.
/// </summary>
public class UserFilterViewModel
{
    /// <summary>
    /// Gets or sets whether to include inactive users.
    /// </summary>
    [Display(Name = "Include Inactive Users")]
    public bool IncludeInactive { get; set; }

    /// <summary>
    /// Gets or sets the role filter.
    /// </summary>
    [Display(Name = "Role")]
    public string? RoleFilter { get; set; }

    /// <summary>
    /// Gets or sets the search term.
    /// </summary>
    [Display(Name = "Search")]
    [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Gets or sets the sort field.
    /// </summary>
    [Display(Name = "Sort By")]
    public string SortBy { get; set; } = "Username";

    /// <summary>
    /// Gets or sets whether to sort descending.
    /// </summary>
    [Display(Name = "Sort Descending")]
    public bool SortDescending { get; set; }
}
