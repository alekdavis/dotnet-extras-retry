using DotNetExtras.Retry;

namespace RetryDemo;
internal partial class Program
{
    /// <summary>
    /// Illustrates how to retry an operation that can fail for different reasons,
    /// each handled by its own retry rule.
    /// </summary>
    /// <param name="throttleTimes">
    /// Number of times the server throttles the request (returns 429) before it succeeds.
    /// </param>
    /// <param name="maxThrottleAttempts">
    /// The maximum number of attempts attributed to the throttling rule.
    /// </param>
    /// <param name="totalTimeoutMilliseconds">
    /// Optional cap on the total time of the operation (0 means no cap).
    /// </param>
    /// <remarks>
    /// The first rule reloads the client secret when it expires (one retry).
    /// The second rule retries when the server returns 429, waiting for the time
    /// specified by the <c>Retry-After</c> header. Each rule has its own budget,
    /// so a reload does not consume throttling retries and vice versa.
    /// When the throttling rule is exhausted, the last result (429) is returned.
    /// </remarks>
    internal static void RulesRetryDemo
    (
        int throttleTimes,
        int maxThrottleAttempts,
        int totalTimeoutMilliseconds = 0
    )
    {
        HttpService service = new()
        {
            // This is just for the simulation purposes.
            ThrottleTimes = throttleTimes,
        };

        try
        {
            HttpResult result = Execute.WithRetry
            (
                service.Send,
                [
                    // Rule 1: expired client secret -> reload and retry once.
                    new RetryRule<HttpResult>
                    {
                        OnException = ex => ex is SecretExpiredException,
                        Reload      = service,
                        Attempts    = 2
                    },

                    // Rule 2: HTTP 429 -> wait for Retry-After (or 50 ms) and retry.
                    new RetryRule<HttpResult>
                    {
                        OnResult = r => r.StatusCode == 429,
                        Attempts = maxThrottleAttempts,
                        GetSleep = (r, ex) => r?.RetryAfter ?? TimeSpan.FromMilliseconds(50)
                    }
                ],
                logger: null,

                // Optional cap on the total time across all rules.
                totalTimeout: totalTimeoutMilliseconds > 0 
                    ? TimeSpan.FromMilliseconds(totalTimeoutMilliseconds) 
                    : null
            );

            Console.WriteLine(result.StatusCode == 200 ? "SUCCESS." : "ERROR.");
        }
        catch
        {
            Console.WriteLine("ERROR.");
        }
    }
}
