namespace JupiterDMS.Domain.Enums;

/// <summary>
/// Defines the checkout status of a document.
/// </summary>
public enum CheckoutStatus
{
    /// <summary>
    /// Document is available for checkout.
    /// </summary>
    Available = 0,

    /// <summary>
    /// Document is checked out by a user.
    /// </summary>
    CheckedOut = 1,

    /// <summary>
    /// Document checkout has expired.
    /// </summary>
    CheckoutExpired = 2,

    /// <summary>
    /// Document is locked by system administrator.
    /// </summary>
    Locked = 3
}
