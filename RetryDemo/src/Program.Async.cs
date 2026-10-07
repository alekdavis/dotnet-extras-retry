using DotNetExtras.Retry;

namespace RetryDemo;
internal partial class Program
{
    /// <summary>
    /// Illustrates how to retry an asynchronous operation using retry rules,
    /// an asynchronous reload, and a cancellation token.
    /// </summary>
    /// <param name="throttleTimes">
    /// Number of times the server throttles the request (returns 429) before it succeeds.
    /// </param>
    /// <param name="cancelAfterMilliseconds">
    /// Time after which the whole operation is cancelled.
    /// </param>
    /// <remarks>
    /// Because <see cref="HttpService"/> implements <see cref="IReloadableAsync"/>,
    /// the secret is reloaded via <see cref="HttpService.ReloadAsync"/>.
    /// The cancellation token caps the total duration of the operation:
    /// cancellation is never retried and results in an <see cref="OperationCanceledException"/>.
    /// </remarks>
    internal static async Task AsyncRetryDemo
    (
        int throttleTimes,
        int cancelAfterMilliseconds
    )
    {
        HttpService service = new()
        {
            // This is just for the simulation purposes.
            ThrottleTimes = throttleTimes,
        };

        using CancellationTokenSource cts = new();
        cts.CancelAfter(cancelAfterMilliseconds);

        try
        {
            HttpResult result = await Execute.WithRetryAsync
            (
                () => service.SendAsync(cts.Token),
                [
                    // Rule 1: expired client secret -> reload asynchronously and retry once.
                    new RetryRule<HttpResult>
                    {
                        OnException = ex => ex is SecretExpiredException,
                        ReloadAsync = service,
                        Attempts    = 2
                    },

                    // Rule 2: HTTP 429 -> wait for Retry-After and retry.
                    new RetryRule<HttpResult>
                    {
                        OnResult = r => r.StatusCode == 429,
                        Attempts = 10,
                        GetSleep = (r, ex) => r?.RetryAfter
                    }
                ],
                logger: null,
                cancellationToken: cts.Token
            );

            Console.WriteLine(result.StatusCode == 200 ? "SUCCESS." : "ERROR.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("CANCELLED.");
        }
        catch
        {
            Console.WriteLine("ERROR.");
        }
    }
}
