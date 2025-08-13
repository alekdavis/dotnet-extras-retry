using DotNetExtras.Retry;

namespace RetryDemo;
internal partial class Program
{
    internal static void SimpleRetryAfterReloadDemo
    (
        int attempt
    )
    {
        ReloadableService service = new()
        {
            Attempt = attempt,
        };

        try
        {
            Execute.WithRetry<InvalidOperationException>
            (
                service.DoSomething,
                service
            );

            Console.WriteLine("SUCCESS.");
        }
        catch
        {
            Console.WriteLine("ERROR.");
        }
    }

    internal static void ComplexRetryAfterReloadDemo
    (
        int attempt
    )
    {
        ReloadableService service = new()
        {
            Attempt = attempt,
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
                    throw new NotSupportedException("Simulated exception triggering reload.", ex);
                }

            }, service);

            Console.WriteLine("SUCCESS.");
        }
        catch
        {
            Console.WriteLine("ERROR.");
        }
    }
}
