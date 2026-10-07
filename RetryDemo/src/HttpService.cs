using DotNetExtras.Retry;

namespace RetryDemo;
/// <summary>
/// Simulates an exception thrown when the client secret expires.
/// </summary>
internal class SecretExpiredException: Exception
{
    internal SecretExpiredException(): base("Client secret expired.") { }
}

/// <summary>
/// Simulates an HTTP response returned by <see cref="HttpService"/>.
/// </summary>
/// <param name="StatusCode">
/// HTTP status code, such as 200 (OK) or 429 (Too Many Requests).
/// </param>
/// <param name="RetryAfter">
/// Value of the <c>Retry-After</c> header (if any).
/// </param>
internal record HttpResult(int StatusCode, TimeSpan? RetryAfter = null);

/// <summary>
/// Simulates an HTTP client that may fail because the client secret has expired
/// (requires a reload) or because the server is throttling requests (HTTP 429).
/// </summary>
/// <remarks>
/// The service implements both <see cref="IReloadable"/> and <see cref="IReloadableAsync"/>,
/// so the synchronous retry methods call <see cref="Reload"/> and the asynchronous
/// retry methods call <see cref="ReloadAsync"/>.
/// </remarks>
internal class HttpService: IReloadable, IReloadableAsync
{
    private bool _secretExpired = true;
    private int _throttledCount = 0;

    /// <summary>
    /// Number of times the server throttles the request before it succeeds
    /// (the value is used in the simulation of a failed and successful call).
    /// </summary>
    internal int ThrottleTimes { get; set; } = 2;

    /// <summary>
    /// Value of the simulated <c>Retry-After</c> header.
    /// </summary>
    internal TimeSpan RetryAfter { get; set; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Simulates a synchronous HTTP request.
    /// </summary>
    internal HttpResult Send()
    {
        Console.WriteLine("Sending the request.");

        if (_secretExpired)
        {
            Console.WriteLine("The client secret has expired.");
            throw new SecretExpiredException();
        }

        if (_throttledCount < ThrottleTimes)
        {
            _throttledCount++;
            Console.WriteLine($"The server returned 429 (Retry-After: {RetryAfter.TotalMilliseconds} ms).");
            return new HttpResult(429, RetryAfter);
        }

        Console.WriteLine("The server returned 200.");
        return new HttpResult(200);
    }

    /// <summary>
    /// Simulates an asynchronous HTTP request.
    /// </summary>
    internal async Task<HttpResult> SendAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        return Send();
    }

    /// <summary>
    /// Simulates reloading the client secret synchronously.
    /// </summary>
    public void Reload()
    {
        Console.WriteLine("Reloading the client secret (synchronously).");
        _secretExpired = false;
    }

    /// <summary>
    /// Simulates reloading the client secret asynchronously (e.g., from a key vault).
    /// </summary>
    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Reloading the client secret (asynchronously).");
        await Task.Delay(10, cancellationToken);
        _secretExpired = false;
    }
}
