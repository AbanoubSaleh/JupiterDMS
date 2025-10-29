using JupiterDMS.Domain.Common;
using JupiterDMS.Domain.Enums;

namespace JupiterDMS.Domain.Entities;

/// <summary>
/// Represents an audit log entry in the document management system.
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// Gets or sets the type of action performed.
    /// </summary>
    public AuditActionType ActionType { get; set; }

    /// <summary>
    /// Gets or sets the type of entity affected.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the entity affected.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Gets or sets the email of the user who performed the action.
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the action occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the description of the action.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional data as JSON string.
    /// </summary>
    public string? AdditionalDataJson { get; set; }

    /// <summary>
    /// Gets or sets the IP address from which the action was performed.
    /// </summary>
    public string? IpAddress { get; set; }


}

