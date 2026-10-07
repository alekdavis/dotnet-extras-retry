namespace DotNetExtras.Retry;
/// <summary>
/// Defines the asynchronous method allowing an object to re-initialize.
/// </summary>
/// <remarks>
/// This interface is used by the <see cref="O:DotNetExtras.Retry.Execute.WithRetryAsync"/> methods
/// when the retry condition is met and the object state or configuration must be refreshed
/// asynchronously (e.g., by fetching a new secret from a remote vault) before retrying a failed operation.
/// If an object implements both <see cref="IReloadable"/> and <see cref="IReloadableAsync"/>,
/// the asynchronous retry methods call <see cref="ReloadAsync"/>.
/// </remarks>
public interface IReloadableAsync
{
    /// <summary>
    /// Asynchronously reinitializes the object (with potentially updated settings).
    /// </summary>
    /// <param name="cancellationToken">
    /// Token that cancels the operation.
    /// </param>
    Task ReloadAsync(CancellationToken cancellationToken = default);
}
