namespace DotNetExtras.Retry;
/// <summary>
/// Defines the method allowing an object to re-initialize.
/// </summary>
/// <remarks>
/// This interface is used by the relevant <see cref="O:DotNetExtras.Retry.Execute.WithRetry"/> methods
/// when the retry condition is met and the object state or configuration must be refreshed
/// before retrying a failed operation.
/// </remarks>
public interface IReloadable
{
    /// <summary>
    /// Reinitializes the object (with potentially updated settings).
    /// </summary>
    /// <remarks>
    /// A typical use case for this method is reload configuration settings,
    /// that may have changed since the object was created or last used,
    /// such as client credentials, passwords, keys, and so on.
    /// </remarks>
    void Reload();
}
