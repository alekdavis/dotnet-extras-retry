using DotNetExtras.Retry;

namespace RetryDemo;
/// <summary>
/// Illustrates a reloadable service that performs operations which may fail.
/// </summary>
/// <remarks>
/// This serves implements the reload functionality.
/// </remarks>
internal class ReloadableService: IReloadable
{
    /// <summary>
    /// Gets or sets the current attempt count for an operation
    /// (the value is used in the simulation of a failed and successful call).
    /// </summary>
    internal int Attempt { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of failed attempts after which recovery actions should be initiated
    /// (the value is used in the simulation of a failed and successful call).
    /// </summary>
    internal int RecoverAfterAttempt { get; set; } = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReloadableService"/> class.
    /// </summary>
    internal ReloadableService()
    {
        Initialize();
    }

    /// <summary>
    /// Initializes both the reloadable and non-reloadable settings.
    /// </summary>
    internal void Initialize()
    {
        Console.WriteLine("Initializing non-reloadable settings.");

        Reload();
    }

    /// <summary>
    /// Initializes the reloadable settings.
    /// </summary>
    /// <remarks>
    /// This method gets called when the retry condition is met.
    /// It is supposed to re-initialize the object state or configuration,
    /// assuming that a failed operation can be fixed after reloading the settings.
    /// </remarks>
    public void Reload()
    {
        Console.WriteLine("Initializing reloadable settings.");
    }

    /// <summary>
    /// Simulates an operation that may fail and require a retry.
    /// </summary>
    /// <remarks>
    /// This method does not return a value and is used to illustrate a simple scenario.
    /// </remarks>
    internal void DoSomething()
    {
        Console.WriteLine($"Performing the operation.");

        // Simulate a recoverable error based on the attempt count
        // assuming that we only retry once.
        if (Attempt < 2)
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

        // Simulate a recoverable error based on the attempt count
        // assuming that we only retry once.
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
