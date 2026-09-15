namespace RetryDemo;
/// <summary>
/// Illustrates a service that performs operations which may fail.
/// </summary>
/// <remarks>
/// This serves does not implement the reload functionality
/// because it is not used in the reload and retry demo.
/// </remarks>
internal class Service
{
    /// <summary>
    /// Holds the start time for the purpose of calculating the timeout
    /// (the value is used in the simulation of a failed and successful call).
    /// </summary>
    private readonly DateTime _startTime = DateTime.Now;

    /// <summary>
    /// Holds the duration to wait before attempting recovery after a timeout occurs
    /// (the value is used in the simulation of a failed and successful call).
    /// </summary>
    internal TimeSpan RecoverAfterTimeout { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Holds the number of failed attempts after which recovery actions should be triggered
    /// (the value is used in the simulation of a failed and successful call).
    /// </summary>
    internal int RecoverAfterAttempt { get; set; } = 1;

    /// <summary>
    /// Holds the current attempt count for an operation.
    /// </summary>
    internal int Attempt { get; set; } = 1;

    /// <summary>
    /// Simulates an operation that may fail and require a retry.
    /// </summary>
    /// <remarks>
    /// This method does not return a value and is used to illustrate a simple scenario.
    /// </remarks>
    internal void DoSomething()
    {
        Console.WriteLine($"Performing the operation.");

        if ((RecoverAfterAttempt > 0 && Attempt < RecoverAfterAttempt) ||
            (RecoverAfterTimeout > TimeSpan.Zero && DateTime.Now - _startTime < RecoverAfterTimeout))
        {
            Attempt++;
            Console.WriteLine("A potentially recoverable error occurred.");
            throw new InvalidOperationException("Expected error.");
        }

        Console.WriteLine("Completed the operation.");
    }

    /// <summary>
    /// Simulates an operation that may fail and require a retry.
    /// </summary>
    /// <remarks>
    /// This method returns a value and is used to illustrate a complex scenario.
    /// </remarks>
    internal int DoSomethingElse()
    {
        Console.WriteLine($"Performing the operation.");

        if ((RecoverAfterAttempt > 0 && Attempt < RecoverAfterAttempt) ||
            (RecoverAfterTimeout > TimeSpan.Zero && DateTime.Now - _startTime < RecoverAfterTimeout))
        {
            Attempt++;
            Console.WriteLine("A potentially recoverable error occurred.");
            throw new InvalidOperationException("Unexpected error.");
        }

        Console.WriteLine($"Completed the operation.");

        return 0;
    }

    /// <summary>
    /// Simulates an operation that always completes but may return a result
    /// indicating that it is not ready yet.
    /// </summary>
    /// <returns>
    /// <c>Pending</c> while the operation is not ready yet; otherwise, <c>Ready</c>.
    /// </returns>
    /// <remarks>
    /// This method never throws; it is used to illustrate a predicate-based retry,
    /// where the retry condition is determined by inspecting the returned value.
    /// </remarks>
    internal string GetStatus()
    {
        Console.WriteLine("Checking the operation status.");

        if (Attempt < RecoverAfterAttempt)
        {
            Attempt++;
            Console.WriteLine("The operation is still pending.");
            return "Pending";
        }

        Console.WriteLine("The operation is ready.");
        return "Ready";
    }

    /// <summary>
    /// Simulates an operation that may fail in two different ways:
    /// by throwing a transient exception (hard failure) or by returning
    /// a throttled result (soft failure).
    /// </summary>
    /// <returns>
    /// <c>Throttled</c> when the request is throttled; otherwise, <c>OK</c>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown to simulate a transient error.
    /// </exception>
    /// <remarks>
    /// This method is used to illustrate a combined retry, where the retry can be
    /// triggered by either an exception or a custom condition (predicate).
    /// </remarks>
    internal string SendRequest()
    {
        Console.WriteLine("Sending the request.");

        if (Attempt < RecoverAfterAttempt)
        {
            // Alternate between a hard failure (exception) and
            // a soft failure (throttled result) to exercise both retry triggers.
            bool transient = Attempt % 2 == 1;

            Attempt++;

            if (transient)
            {
                Console.WriteLine("A transient error occurred.");
                throw new InvalidOperationException("Expected transient error.");
            }

            Console.WriteLine("The request was throttled.");
            return "Throttled";
        }

        Console.WriteLine("The request succeeded.");
        return "OK";
    }
}
