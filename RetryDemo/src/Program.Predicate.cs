using DotNetExtras.Retry;

namespace RetryDemo;
internal partial class Program
{
    /// <summary>
    /// Illustrates how to retry an operation based on a custom condition (predicate)
    /// that inspects the returned result instead of catching an exception.
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
    /// In this example, the operation never throws; the retry is triggered
    /// when the returned status indicates that the result is not ready yet.
    /// </remarks>
    internal static void PredicateRetryDemo
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

        // The operation does not throw; instead, it returns a status value.
        // Retry while the status is "Pending", up to the max number of attempts.
        string status = Execute.WithRetry<string>
        (
            // This operation call can be a more complex delegate,
            // as illustrated in another example.
            service.GetStatus,

            // Retry condition: retry while the result is not ready yet.
            result => result == "Pending",

            // The max number of all attempts.
            maxAttempts,

            // Delay between retries.
            TimeSpan.FromMilliseconds(sleepMilliseconds)
        );

        // When the attempts are exhausted, the last result is returned
        // (it may still satisfy the retry condition), so inspect it before using it.
        if (status == "Ready")
        {
            Console.WriteLine("SUCCESS.");
        }
        else
        {
            Console.WriteLine("ERROR.");
        }
    }
}
