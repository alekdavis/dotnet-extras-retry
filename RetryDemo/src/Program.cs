namespace RetryDemo;
/// <summary>
/// Illustrates the use of the `DotNetExtras.Retry` library for retrying failed operations.
/// </summary>
internal partial class Program
{
    /// <summary>
    /// Demonstrates various retry mechanisms and outcomes under different scenarios,
    /// including recovery after reload, retrying a fixed number of times, 
    /// retrying until a timeout is reached, retrying based on a custom condition
    /// (predicate), and retrying on either an exception or a custom condition
    /// (combined mode), retrying on multiple conditions using retry rules,
    /// and retrying asynchronous operations.
    /// The samples cover the cases when the retries solve the problem and when they do not.
    /// </summary>
    internal static async Task Main()
    {
        // ---
        // RELOAD AND RETRY
        // ---

        // The following demos illustrate how to handle a failed operation
        // after reloading the application configuration and retrying once.
        // The samples cover a simple scenario when the operation does not return 
        // a value and the retry condition is triggered right by the operation,
        // as well as a complex scenario when the operation returns a value
        // and the retry condition is determined via the custom logic.
        // For each of the two scenarios, the samples demonstrate both
        // the successful and unsuccessful retries.

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
        // TRY THREE TIMES
        // ---

        // The following demos illustrate how to handle a failed operation
        // for three attempts.
        // The samples cover a simple scenario when the operation does not return 
        // a value and the retry condition is triggered right by the operation,
        // as well as a complex scenario when the operation returns a value
        // and the retry condition is determined via the custom logic.
        // For each of the two scenarios, the samples demonstrate both
        // the successful and unsuccessful retries.

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

        // The following demos illustrate how to handle a failed operation
        // until a timeout is reached.
        // The samples cover a simple scenario when the operation does not return 
        // a value and the retry condition is triggered right by the operation,
        // as well as a complex scenario when the operation returns a value
        // and the retry condition is determined via the custom logic.
        // For each of the two scenarios, the samples demonstrate both
        // the successful and unsuccessful retries.

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

        // ---
        // RETRY BASED ON A CUSTOM CONDITION (PREDICATE)
        // ---

        // The following demos illustrate how to retry an operation that does not
        // throw an exception, but instead returns a result indicating whether
        // the operation has succeeded. The retry is triggered by a predicate that
        // inspects the returned value. The samples demonstrate both the successful
        // and unsuccessful retries.

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("PREDICATE RECOVERY ON THE THIRD ATTEMPT");
        Console.WriteLine("(result becomes ready on the third attempt)");
        Console.WriteLine("-----------------------------------------");
        PredicateRetryDemo(4, 3, 100);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("PREDICATE FAILURE AFTER TWO ATTEMPTS");
        Console.WriteLine("(result never becomes ready in time)");
        Console.WriteLine("-----------------------------------------");
        PredicateRetryDemo(2, 4, 100);

        // ---
        // RETRY ON AN EXCEPTION OR A CUSTOM CONDITION (COMBINED MODE)
        // ---

        // The following demos illustrate how to retry an operation that can fail
        // in two different ways: by throwing an exception (hard failure) or by
        // returning an unacceptable result (soft failure). The retry is triggered
        // by either signal. The samples demonstrate a successful retry, a failure
        // where the last attempt returns an unacceptable result, and a failure
        // where the last attempt throws an exception (which is rethrown).

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("COMBINED RECOVERY ON THE THIRD ATTEMPT");
        Console.WriteLine("(retry succeeds after both failure types)");
        Console.WriteLine("-----------------------------------------");
        CombinedRetryDemo(4, 3, 100);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("COMBINED FAILURE (LAST RESULT UNACCEPTABLE)");
        Console.WriteLine("(the last attempt returns a throttled result)");
        Console.WriteLine("-----------------------------------------");
        CombinedRetryDemo(2, 5, 100);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("COMBINED FAILURE (LAST ATTEMPT THROWS)");
        Console.WriteLine("(the last attempt throws the expected exception)");
        Console.WriteLine("-----------------------------------------");
        CombinedRetryDemo(3, 5, 100);

        // ---
        // RETRY ON MULTIPLE CONDITIONS (RULES)
        // ---

        // The following demos illustrate how to handle different failures with
        // different retry rules: an expired client secret is fixed by a reload,
        // while HTTP 429 (Too Many Requests) is retried after the Retry-After delay.
        // Each rule has its own retry budget.

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("RULES RECOVERY");
        Console.WriteLine("(reload fixes the secret, throttling stops)");
        Console.WriteLine("-----------------------------------------");
        RulesRetryDemo(2, 5);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("RULES FAILURE (THROTTLING BUDGET EXHAUSTED)");
        Console.WriteLine("(the last result is still 429)");
        Console.WriteLine("-----------------------------------------");
        RulesRetryDemo(5, 3);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("RULES FAILURE (TOTAL TIMEOUT REACHED)");
        Console.WriteLine("(the total timeout stops the retries)");
        Console.WriteLine("-----------------------------------------");
        RulesRetryDemo(10, 20, 250);

        // ---
        // ASYNCHRONOUS RETRY
        // ---

        // The following demos illustrate how to retry an asynchronous operation
        // with an asynchronous reload and a cancellation token that caps
        // the total duration of the operation.

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("ASYNC RECOVERY");
        Console.WriteLine("(async reload fixes the secret, throttling stops)");
        Console.WriteLine("-----------------------------------------");
        await AsyncRetryDemo(2, 2000);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("ASYNC CANCELLATION");
        Console.WriteLine("(the operation is cancelled before it succeeds)");
        Console.WriteLine("-----------------------------------------");
        await AsyncRetryDemo(10, 300);
    }
}
