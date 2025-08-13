using DotNetExtras.Retry;

namespace RetryDemo;
internal class ReloadableService: IReloadable
{
    internal int Attempt { get; set; } = 1;

    internal int RecoverAfterAttempt { get; set; } = 1;

    internal ReloadableService()
    {
        Initialize();
    }

    internal void Initialize()
    {
        Console.WriteLine("Initializing non-reloadable settings.");

        Reload();
    }

    public void Reload()
    {
        Console.WriteLine("Initializing reloadable settings.");
    }

    internal void DoSomething()
    {
        Console.WriteLine($"Performing the operation.");

        if (Attempt < 2)
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

        if (Attempt < 2)
        {
            Attempt++;
            Console.WriteLine("A potentially recoverable error occurred.");
            throw new InvalidOperationException("Unexpected error.");
        }

        Console.WriteLine($"Completed the operation.");

        return 0;
    }
}
