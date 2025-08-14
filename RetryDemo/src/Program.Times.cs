using DotNetExtras.Retry;

namespace RetryDemo;
internal partial class Program
{
    /// <summary>
    /// Illustrates how to call an operation that does not return a value
    /// and retry it a fixed number of times if the operation fails.
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
    /// In this example, the retry condition is triggered right by the operation.
    /// </remarks>
    internal static void SimpleRetryTimesDemo
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

        // This try block is to simulate a non-recoverable failure.
        try
        {
            // If the operation throws an InvalidOperationException,
            // keep retrying with a delay between retries
            // for the total of max number of attempts.
            Execute.WithRetry<InvalidOperationException>
            (
                // This operation call can be a more complex delegate,
                // as illustrated in another example.
                service.DoSomething, 

                // The max number of all attempts.
                maxAttempts,

                // Delay between retries.
                TimeSpan.FromMilliseconds(sleepMilliseconds)
            );

            Console.WriteLine("SUCCESS.");
        }
        catch
        {
            Console.WriteLine("ERROR.");
        }
    }

    /// <summary>
    /// Illustrates how to call an operation that returns a value
    /// and retry it a fixed number of times if the operation fails.
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
    /// In this example, the retry condition is determined via the custom logic.
    /// </remarks>
    internal static void ComplexRetryTimesDemo
    (
        int maxRetries,
        int recoverAfterAttempt,
        int sleepMilliseconds
    )
    {
        Service service = new()
        {
            // This is just for the simulation purposes.
            RecoverAfterAttempt = recoverAfterAttempt,
        };

        // This try block is to handle a simulation of a non-recoverable failure.
        try
        {
            // If the operation throws a NotSupportedException,
            // keep retrying with a delay between retries
            // for the total of max number of attempts.
            // The operation is expected to return an int value.
            int result = Execute.WithRetry<NotSupportedException, int>(() => 
            {
                // BEGINNING OF THE CODE BLOCK THAT WILL BE RETRIED.
                try
                {
                    return service.DoSomethingElse();
                }
                // This is not the expected exception for the retry,
                // but...
                catch (InvalidOperationException ex)
                {
                    // ...we can simulate the expected exception for an appropriate condition.
                    if (ex.Message.StartsWith("Unexpected"))
                    {
                        throw new NotSupportedException("Simulated exception triggering a retry.", ex);
                    }

                    throw;
                }
                // END OF THE CODE BLOCK THAT WILL BE RETRIED.

            }, maxRetries, TimeSpan.FromMilliseconds(sleepMilliseconds));

            Console.WriteLine("SUCCESS.");
        }
        catch
        {
            Console.WriteLine("ERROR.");
        }
    }
}
