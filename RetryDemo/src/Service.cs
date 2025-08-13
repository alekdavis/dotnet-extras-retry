namespace RetryDemo;
internal class Service
{
    private readonly DateTime _startTime = DateTime.Now;

    internal TimeSpan RecoverAfterTimeout { get; set; } = TimeSpan.Zero;

    internal int RecoverAfterAttempt { get; set; } = 1;

    internal int Attempt { get; set; } = 1;

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
