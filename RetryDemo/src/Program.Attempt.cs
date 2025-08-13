using DotNetExtras.Retry;

namespace RetryDemo;
internal partial class Program
{
    internal static void SimpleRetryTimesDemo
    (
        int maxRetries, 
        int recoverAfterAttempt,
        int sleepMilliseconds
    )
    {
        Service service = new()
        {
            RecoverAfterAttempt = recoverAfterAttempt,
        };

        try
        {
            Execute.WithRetry<InvalidOperationException>
            (
                service.DoSomething, 
                maxRetries,
                TimeSpan.FromMilliseconds(sleepMilliseconds)
            );

            Console.WriteLine("SUCCESS.");
        }
        catch
        {
            Console.WriteLine("ERROR.");
        }
    }

    internal static void ComplexRetryTimesDemo
    (
        int maxRetries,
        int recoverAfterAttempt,
        int sleepMilliseconds
    )
    {
        Service service = new()
        {
            RecoverAfterAttempt = recoverAfterAttempt,
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

            }, maxRetries, TimeSpan.FromMilliseconds(sleepMilliseconds));

            Console.WriteLine("SUCCESS.");
        }
        catch
        {
            Console.WriteLine("ERROR.");
        }
    }
}
