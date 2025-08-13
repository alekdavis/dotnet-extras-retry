namespace RetryDemo;
internal partial class Program
{
    internal static void Main()
    {
        // ---
        // RETRY AFTER A RELOAD
        // ---

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("SIMPLE RECOVERY AFTER A RELOAD");
        Console.WriteLine("(reload fixes the issue)");
        Console.WriteLine("-----------------------------------------");
        SimpleRetryAfterReloadDemo(1);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("SIMPLE FAILURE AFTER A RELOAD");
        Console.WriteLine("(reload does not fix the issue)");
        Console.WriteLine("-----------------------------------------");
        SimpleRetryAfterReloadDemo(0);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("COMPLEX RECOVERY AFTER A RELOAD");
        Console.WriteLine("(reload fixes the issue)");
        Console.WriteLine("-----------------------------------------");
        ComplexRetryAfterReloadDemo(1);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("COMPLEX FAILURE AFTER A RELOAD");
        Console.WriteLine("(reload does not fix the issue)");
        Console.WriteLine("-----------------------------------------");
        ComplexRetryAfterReloadDemo(0);

        // ---
        // RETRY 3 TIMES
        // ---

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("SIMPLE RECOVERY ON THE THIRD ATTEMPT");
        Console.WriteLine("(retry succeeds on the third attempt)");
        Console.WriteLine("-----------------------------------------");
        SimpleRetryTimesDemo(4, 3, 100);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("SIMPLE FAILURE AFTER THREE ATTEMPTS");
        Console.WriteLine("(retry fails after the third attempt)");
        Console.WriteLine("-----------------------------------------");
        SimpleRetryTimesDemo(3, 4, 100);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("COMPLEX RECOVERY ON THE THIRD ATTEMPT");
        Console.WriteLine("(retry succeeds on the third attempt)");
        Console.WriteLine("-----------------------------------------");
        ComplexRetryTimesDemo(4, 3, 100);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("COMPLEX FAILURE AFTER THREE ATTEMPTS");
        Console.WriteLine("(retry fails after the third attempt)");
        Console.WriteLine("-----------------------------------------");
        ComplexRetryTimesDemo(3, 4, 100);

        // ---
        // RETRY UNTIL TIMEOUT
        // ---

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("SIMPLE RECOVERY BEFORE THE TIMEOUT");
        Console.WriteLine("(retry succeeds before a timeout)");
        Console.WriteLine("-----------------------------------------");
        SimpleRetryBeforeTimeoutDemo(900, 700, 50);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("SIMPLE FAILURE AFTER THE TIMEOUT");
        Console.WriteLine("(retry fails after a timeout)");
        Console.WriteLine("-----------------------------------------");
        SimpleRetryBeforeTimeoutDemo(700, 900, 50);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("COMPLEX RECOVERY BEFORE THE TIMEOUT");
        Console.WriteLine("(retry succeeds before a timeout)");
        Console.WriteLine("-----------------------------------------");
        ComplexRetryBeforeTimeoutDemo(900, 700, 50);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("COMPLEX FAILURE AFTER THE TIMEOUT");
        Console.WriteLine("(retry fails after a timeout)");
        Console.WriteLine("-----------------------------------------");
        ComplexRetryBeforeTimeoutDemo(700, 900, 50);
    }
}
