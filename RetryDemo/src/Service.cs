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
}
