using DotNetExtras.Retry;

namespace RetryDemo;
internal partial class Program
{
    internal static void SimpleRetryBeforeTimeoutDemo
    (
        int timeout, 
        int recoverAfterTimeout,
        int sleepMilliseconds
    )
    {
        Service service = new()
        {
            RecoverAfterTimeout = TimeSpan.FromMilliseconds(recoverAfterTimeout),
        };

        try
        {
            Execute.WithRetry<InvalidOperationException>
            (
                service.DoSomething, 
                TimeSpan.FromMilliseconds(timeout),
                TimeSpan.FromMilliseconds(sleepMilliseconds)
            );

            Console.WriteLine("SUCCESS.");
        }
        catch
        {
            Console.WriteLine("ERROR.");
        }
    }

    internal static void ComplexRetryBeforeTimeoutDemo
    (
        int timeout, 
        int recoverAfterTimeout,
        int sleepMilliseconds
    )
    {
        Service service = new()
        {
            RecoverAfterTimeout = TimeSpan.FromMilliseconds(recoverAfterTimeout),
        };

        try
        {
            int result = Execute.WithRetry<NotSupportedException, int>(() => 
            {
                try
                {
                    return service.DoSomethingElse();
                }
                catch (InvalidOperationException ex)
                {
                    throw new NotSupportedException("Simulated exception triggering a retry.", ex);
                }

            }, TimeSpan.FromMilliseconds(timeout), TimeSpan.FromMilliseconds(sleepMilliseconds));

            Console.WriteLine("SUCCESS.");
        }
        catch
        {
            Console.WriteLine("ERROR.");
        }
    }
}
