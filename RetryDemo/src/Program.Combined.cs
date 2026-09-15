using DotNetExtras.Retry;

namespace RetryDemo;
internal partial class Program
{
    /// <summary>
    /// Illustrates how to retry an operation when it either throws a specific exception
    /// or returns a result that a custom condition (predicate) deems unacceptable.
    /// </summary>
    /// <param name="maxAttempts">
    /// The maximum number of attempts to perform the operation.
    /// </param>
    /// <param name="recoverAfterAttempt">
    /// This number is used to simulate a recovered and failed operation on a retry.
    /// </param>
    /// <param name="sleepMilliseconds">
    /// Delay between retries.
    /// </param>
    /// <remarks>
    /// In this example, the operation can fail in two ways: by throwing a transient
    /// exception (hard failure) or by returning a throttled result (soft failure).
    /// The retry is triggered by either signal. When the attempts are exhausted,
    /// the last failure wins: an exception thrown on the final attempt is rethrown,
    /// while an unacceptable value returned on the final attempt is returned.
    /// </remarks>
    internal static void CombinedRetryDemo
    (
        int maxAttempts,
        int recoverAfterAttempt,
        int sleepMilliseconds
    )
    {
        Service service = new()
        {
            // This is just for the simulation purposes.
            RecoverAfterAttempt = recoverAfterAttempt,
        };

        // This try block handles the case when the last failure is an exception
        // (when the attempts are exhausted, an exception thrown on the final
        // attempt is rethrown).
        try
        {
            // Retry when the operation throws an InvalidOperationException OR
            // when it returns a result that is still marked as "Throttled".
            string result = Execute.WithRetry<InvalidOperationException, string>
            (
                // This operation call can be a more complex delegate,
                // as illustrated in another example.
                service.SendRequest,

                // Retry condition: retry while the result is throttled.
                response => response == "Throttled",

                // The max number of all attempts.
                maxAttempts,

                // Delay between retries.
                TimeSpan.FromMilliseconds(sleepMilliseconds)
            );

            // When the attempts are exhausted without an exception on the final
            // attempt, the last result is returned (it may still be unacceptable).
            if (result == "OK")
            {
                Console.WriteLine("SUCCESS.");
            }
            else
            {
                Console.WriteLine("ERROR.");
            }
        }
        catch
        {
            Console.WriteLine("ERROR.");
        }
    }
}
