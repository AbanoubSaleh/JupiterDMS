namespace JupiterDMS.Domain.Enums;

/// <summary>
/// Defines the status of a document in the system.
/// </summary>
public enum DocumentStatus
{
    /// <summary>
    /// Document is in draft state.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Document is pending review.
    /// </summary>
    PendingReview = 1,

    /// <summary>
    /// Document is approved.
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Document is published and available.
    /// </summary>
    Published = 3,

    /// <summary>
    /// Document is archived.
    /// </summary>
    Archived = 4,

    /// <summary>
    /// Document is obsolete.
    /// </summary>
    Obsolete = 5
}

