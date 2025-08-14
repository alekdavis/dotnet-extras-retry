using DotNetExtras.Retry;

namespace RetryDemo;
internal partial class Program
{
    /// <summary>
    /// Illustrates how to call an operation that does not return a value
    /// and retry it after reloading the configuration.
    /// </summary>
    /// <param name="attempt">
    /// This number is used to simulate a recovered and failed operation on a retry.
    /// </param>
    /// <remarks>
    /// In this example, the retry condition is triggered right by the operation.
    /// </remarks>

    internal static void SimpleRetryAfterReloadDemo
    (
        int attempt
    )
    {
        ReloadableService service = new()
        {
            // This is just for the simulation purposes.
            Attempt = attempt,
        };

        // This try block is to simulate a non-recoverable failure.
        try
        {
            // If the operation throws an InvalidOperationException,
            // reload the service object and retry the operation one more time.
            Execute.WithRetry<InvalidOperationException>
            (
                // This operation call can be a more complex delegate,
                // as illustrated in another example.
                service.DoSomething,

                // We are passing the same service object here because it is the one that 
                // implements the reload method, but it can be a different object.
                service
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
    /// and retry it after reloading the configuration.
    /// </summary>
    /// <param name="attempt">
    /// This number is used to simulate a recovered and failed operation on a retry.
    /// </param>
    /// <remarks>
    /// In this example, the retry condition is determined via the custom logic.
    /// </remarks>
    internal static void ComplexRetryAfterReloadDemo
    (
        int attempt
    )
    {
        ReloadableService service = new()
        {
            // This is just for the simulation purposes.
            Attempt = attempt,
        };

        // This try block is to handle a simulation of a non-recoverable failure.
        try
        {
            // If the operation throws a NotSupportedException,
            // reload the service object and retry the operation one more time.
            // The operation is expected to return an int value.
            int result = Execute.WithRetry<NotSupportedException, int>(() => 
            {
                // BEGINNING OF THE CODE BLOCK THAT WILL BE RETRIED.
                try
                {
                    // Attempt to perform the operation.
                    return service.DoSomethingElse();
                }
                // This is not the expected exception for the retry,
                // but...
                catch (InvalidOperationException ex)
                {
                    // ...we can simulate the expected exception for an appropriate condition.
                    if (ex.Message.StartsWith("Unexpected"))
                    {
                        throw new NotSupportedException("Simulated exception triggering a reload.", ex);
                    }

                    throw;
                }
                // END OF THE CODE BLOCK THAT WILL BE RETRIED.

            }, service);
            // We are passing the same service object here because it is the one that 
            // implements the reload method, but it can be a different object.
            // We use the defaults for the delay (no delay) and the maximum attempts (2).

            Console.WriteLine("SUCCESS.");
        }
        catch
        {
            Console.WriteLine("ERROR.");
        }
    }
}
