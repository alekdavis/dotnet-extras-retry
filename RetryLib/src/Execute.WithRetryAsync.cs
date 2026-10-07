using Microsoft.Extensions.Logging;

namespace DotNetExtras.Retry;

public static partial class Execute
{
    /// <summary>
    /// Asynchronously executes code that returns a value and re-executes it
    /// according to the specified retry rules, each of which defines its own
    /// condition, reload action, retry budget, and delay.
    /// </summary>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Asynchronous code block to be executed.
    /// </param>
    /// <param name="rules">
    /// Retry rules evaluated in order; the first matching rule handles a failure.
    /// </param>
    /// <param name="logger">
    /// Logs retry event information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token that cancels the waits between retries and stops further attempts.
    /// Cancellation is never retried.
    /// </param>
    /// <returns>
    /// Value returned by the code block.
    /// </returns>
    /// <remarks>
    /// See <see cref="RetryRule{T}"/> for the rule semantics.
    /// </remarks>
    public static async Task<T> WithRetryAsync<T>
    (
        Func<Task<T>> code,
        IEnumerable<RetryRule<T>> rules,
        ILogger? logger = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(code);
        RetryRule<T>[] ruleList = ValidateRules(rules);

        int[] retries = new int[ruleList.Length];
        DateTime?[] firstMatch = new DateTime?[ruleList.Length];

        bool IsExhausted(int index)
        {
            firstMatch[index] ??= DateTime.UtcNow;

            return ruleList[index].IsExhausted(retries[index], firstMatch[index]!.Value);
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            T result;
            RetryRule<T> rule;

            try
            {
                result = await code().ConfigureAwait(false);
            }
            catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))
            {
                int index = Array.FindIndex(ruleList, r => r.Matches(ex));

                if (index < 0 || IsExhausted(index))
                {
                    throw;
                }

                retries[index]++;
                rule = ruleList[index];

                await PrepareAsync(ex.GetType(), rule.GetSleepTime(default, ex), rule.Reload, logger, cancellationToken, rule.ReloadAsync)
                    .ConfigureAwait(false);

                continue;
            }

            int resultIndex = Array.FindIndex(ruleList, r => r.Matches(result));

            if (resultIndex < 0 || IsExhausted(resultIndex))
            {
                return result;
            }

            retries[resultIndex]++;
            rule = ruleList[resultIndex];

            await PrepareAsync(null, rule.GetSleepTime(result, null), rule.Reload, logger, cancellationToken, rule.ReloadAsync)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously executes code that does not return a value and,
    /// if the code throws a specific exception, calls the
    /// <see cref="IReloadable.Reload"/> method (if the caller is specified)
    /// and re-executes it until it runs out of attempts.
    /// </summary>
    /// <typeparam name="E">
    /// Exception type that triggers code re-execution.
    /// </typeparam>
    /// <param name="code">
    /// Asynchronous code block to be executed.
    /// </param>
    /// <param name="caller">
    /// Optional object implementing the <see cref="IReloadable.Reload"/> method
    /// that will be called on each retry.
    /// </param>
    /// <param name="attempts">
    /// Maximum number of tries.
    /// </param>
    /// <param name="sleep">
    /// Sleep time between retries.
    /// </param>
    /// <param name="logger">
    /// Logs retry event information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token that cancels the waits between retries and stops further attempts.
    /// </param>
    public static Task WithRetryAsync<E>
    (
        Func<Task> code,
        IReloadable? caller,
        int attempts,
        TimeSpan? sleep = null,
        ILogger? logger = null,
        CancellationToken cancellationToken = default
    )
    where E : Exception
    {
        return WithRetryAsync<E, bool>(Wrap(code), caller, attempts, sleep, logger, cancellationToken);
    }

    /// <summary>
    /// Asynchronously executes code that does not return a value and,
    /// if the code throws a specific exception, calls the
    /// <see cref="IReloadable.Reload"/> method (if the caller is specified)
    /// and re-executes it until it runs out of time.
    /// </summary>
    /// <typeparam name="E">
    /// Exception type that triggers code re-execution.
    /// </typeparam>
    /// <param name="code">
    /// Asynchronous code block to be executed.
    /// </param>
    /// <param name="caller">
    /// Optional object implementing the <see cref="IReloadable.Reload"/> method
    /// that will be called on each retry.
    /// </param>
    /// <param name="timeout">
    /// Timeout for all attempts.
    /// </param>
    /// <param name="sleep">
    /// Sleep time between retries.
    /// </param>
    /// <param name="logger">
    /// Logs retry event information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token that cancels the waits between retries and stops further attempts.
    /// </param>
    public static Task WithRetryAsync<E>
    (
        Func<Task> code,
        IReloadable? caller,
        TimeSpan timeout,
        TimeSpan? sleep = null,
        ILogger? logger = null,
        CancellationToken cancellationToken = default
    )
    where E : Exception
    {
        return WithRetryAsync<E, bool>(Wrap(code), caller, timeout, sleep, logger, cancellationToken);
    }

    /// <summary>
    /// Asynchronously executes code that returns a value and,
    /// if the code throws a specific exception, calls the
    /// <see cref="IReloadable.Reload"/> method (if the caller is specified)
    /// and re-executes it until it runs out of attempts.
    /// </summary>
    /// <typeparam name="E">
    /// Exception type that triggers code re-execution.
    /// </typeparam>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Asynchronous code block to be executed.
    /// </param>
    /// <param name="caller">
    /// Optional object implementing the <see cref="IReloadable.Reload"/> method
    /// that will be called on each retry.
    /// </param>
    /// <param name="attempts">
    /// Maximum number of tries.
    /// </param>
    /// <param name="sleep">
    /// Sleep time between retries.
    /// </param>
    /// <param name="logger">
    /// Logs retry event information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token that cancels the waits between retries and stops further attempts.
    /// </param>
    /// <returns>
    /// Value returned by the code block.
    /// </returns>
    public static Task<T> WithRetryAsync<E, T>
    (
        Func<Task<T>> code,
        IReloadable? caller,
        int attempts,
        TimeSpan? sleep = null,
        ILogger? logger = null,
        CancellationToken cancellationToken = default
    )
    where E : Exception
    {
        RetryRule<T> rule = new()
        {
            OnException = ex => ex is E,
            Reload      = caller,
            Attempts    = attempts,
            Sleep       = sleep
        };

        return WithRetryAsync(code, [rule], logger, cancellationToken);
    }

    /// <summary>
    /// Asynchronously executes code that returns a value and,
    /// if the code throws a specific exception, calls the
    /// <see cref="IReloadable.Reload"/> method (if the caller is specified)
    /// and re-executes it until it runs out of time.
    /// </summary>
    /// <typeparam name="E">
    /// Exception type that triggers code re-execution.
    /// </typeparam>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Asynchronous code block to be executed.
    /// </param>
    /// <param name="caller">
    /// Optional object implementing the <see cref="IReloadable.Reload"/> method
    /// that will be called on each retry.
    /// </param>
    /// <param name="timeout">
    /// Timeout for all attempts.
    /// </param>
    /// <param name="sleep">
    /// Sleep time between retries.
    /// </param>
    /// <param name="logger">
    /// Logs retry event information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token that cancels the waits between retries and stops further attempts.
    /// </param>
    /// <returns>
    /// Value returned by the code block.
    /// </returns>
    public static Task<T> WithRetryAsync<E, T>
    (
        Func<Task<T>> code,
        IReloadable? caller,
        TimeSpan timeout,
        TimeSpan? sleep = null,
        ILogger? logger = null,
        CancellationToken cancellationToken = default
    )
    where E : Exception
    {
        RetryRule<T> rule = new()
        {
            OnException = ex => ex is E,
            Reload      = caller,
            Timeout     = timeout,
            Sleep       = sleep
        };

        return WithRetryAsync(code, [rule], logger, cancellationToken);
    }

    /// <summary>
    /// Asynchronously executes code that returns a value and,
    /// if the caller-supplied predicate indicates that the result is not acceptable,
    /// calls the <see cref="IReloadable.Reload"/> method (if the caller is specified)
    /// and re-executes it until it runs out of attempts.
    /// </summary>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Asynchronous code block to be executed.
    /// </param>
    /// <param name="retryCondition">
    /// Predicate that inspects the value returned by the code block and
    /// returns <see langword="true"/> to trigger a retry.
    /// </param>
    /// <param name="caller">
    /// Optional object implementing the <see cref="IReloadable.Reload"/> method
    /// that will be called on each retry.
    /// </param>
    /// <param name="attempts">
    /// Maximum number of tries.
    /// </param>
    /// <param name="sleep">
    /// Sleep time between retries.
    /// </param>
    /// <param name="logger">
    /// Logs retry event information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token that cancels the waits between retries and stops further attempts.
    /// </param>
    /// <returns>
    /// Value returned by the last executed code block.
    /// </returns>
    public static Task<T> WithRetryAsync<T>
    (
        Func<Task<T>> code,
        Func<T, bool> retryCondition,
        IReloadable? caller,
        int attempts,
        TimeSpan? sleep = null,
        ILogger? logger = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(retryCondition);

        RetryRule<T> rule = new()
        {
            OnResult = retryCondition,
            Reload   = caller,
            Attempts = attempts,
            Sleep    = sleep
        };

        return WithRetryAsync(code, [rule], logger, cancellationToken);
    }

    /// <summary>
    /// Asynchronously executes code that returns a value and,
    /// if the caller-supplied predicate indicates that the result is not acceptable,
    /// calls the <see cref="IReloadable.Reload"/> method (if the caller is specified)
    /// and re-executes it until it runs out of time.
    /// </summary>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Asynchronous code block to be executed.
    /// </param>
    /// <param name="retryCondition">
    /// Predicate that inspects the value returned by the code block and
    /// returns <see langword="true"/> to trigger a retry.
    /// </param>
    /// <param name="caller">
    /// Optional object implementing the <see cref="IReloadable.Reload"/> method
    /// that will be called on each retry.
    /// </param>
    /// <param name="timeout">
    /// Timeout for all attempts.
    /// </param>
    /// <param name="sleep">
    /// Sleep time between retries.
    /// </param>
    /// <param name="logger">
    /// Logs retry event information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token that cancels the waits between retries and stops further attempts.
    /// </param>
    /// <returns>
    /// Value returned by the last executed code block.
    /// </returns>
    public static Task<T> WithRetryAsync<T>
    (
        Func<Task<T>> code,
        Func<T, bool> retryCondition,
        IReloadable? caller,
        TimeSpan timeout,
        TimeSpan? sleep = null,
        ILogger? logger = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(retryCondition);

        RetryRule<T> rule = new()
        {
            OnResult = retryCondition,
            Reload   = caller,
            Timeout  = timeout,
            Sleep    = sleep
        };

        return WithRetryAsync(code, [rule], logger, cancellationToken);
    }

    /// <summary>
    /// Asynchronously executes code that returns a value and re-executes it
    /// until it runs out of attempts if the code either throws a specific exception
    /// or the caller-supplied predicate indicates that the result is not acceptable.
    /// </summary>
    /// <typeparam name="E">
    /// Exception type that triggers code re-execution.
    /// </typeparam>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Asynchronous code block to be executed.
    /// </param>
    /// <param name="retryCondition">
    /// Predicate that inspects the value returned by the code block and
    /// returns <see langword="true"/> to trigger a retry.
    /// </param>
    /// <param name="caller">
    /// Optional object implementing the <see cref="IReloadable.Reload"/> method
    /// that will be called on each retry.
    /// </param>
    /// <param name="attempts">
    /// Maximum number of tries.
    /// </param>
    /// <param name="sleep">
    /// Sleep time between retries.
    /// </param>
    /// <param name="logger">
    /// Logs retry event information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token that cancels the waits between retries and stops further attempts.
    /// </param>
    /// <returns>
    /// Value returned by the last executed code block.
    /// </returns>
    /// <remarks>
    /// When the attempts are exhausted, the last failure wins:
    /// an exception is rethrown and a returned value is returned.
    /// </remarks>
    public static Task<T> WithRetryAsync<E, T>
    (
        Func<Task<T>> code,
        Func<T, bool> retryCondition,
        IReloadable? caller,
        int attempts,
        TimeSpan? sleep = null,
        ILogger? logger = null,
        CancellationToken cancellationToken = default
    )
    where E : Exception
    {
        ArgumentNullException.ThrowIfNull(retryCondition);

        RetryRule<T> rule = new()
        {
            OnException = ex => ex is E,
            OnResult    = retryCondition,
            Reload      = caller,
            Attempts    = attempts,
            Sleep       = sleep
        };

        return WithRetryAsync(code, [rule], logger, cancellationToken);
    }

    /// <summary>
    /// Asynchronously executes code that returns a value and re-executes it
    /// until it runs out of time if the code either throws a specific exception
    /// or the caller-supplied predicate indicates that the result is not acceptable.
    /// </summary>
    /// <typeparam name="E">
    /// Exception type that triggers code re-execution.
    /// </typeparam>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Asynchronous code block to be executed.
    /// </param>
    /// <param name="retryCondition">
    /// Predicate that inspects the value returned by the code block and
    /// returns <see langword="true"/> to trigger a retry.
    /// </param>
    /// <param name="caller">
    /// Optional object implementing the <see cref="IReloadable.Reload"/> method
    /// that will be called on each retry.
    /// </param>
    /// <param name="timeout">
    /// Timeout for all attempts.
    /// </param>
    /// <param name="sleep">
    /// Sleep time between retries.
    /// </param>
    /// <param name="logger">
    /// Logs retry event information.
    /// </param>
    /// <param name="cancellationToken">
    /// Token that cancels the waits between retries and stops further attempts.
    /// </param>
    /// <returns>
    /// Value returned by the last executed code block.
    /// </returns>
    /// <remarks>
    /// When the timeout is reached, the last failure wins:
    /// an exception is rethrown and a returned value is returned.
    /// </remarks>
    public static Task<T> WithRetryAsync<E, T>
    (
        Func<Task<T>> code,
        Func<T, bool> retryCondition,
        IReloadable? caller,
        TimeSpan timeout,
        TimeSpan? sleep = null,
        ILogger? logger = null,
        CancellationToken cancellationToken = default
    )
    where E : Exception
    {
        ArgumentNullException.ThrowIfNull(retryCondition);

        RetryRule<T> rule = new()
        {
            OnException = ex => ex is E,
            OnResult    = retryCondition,
            Reload      = caller,
            Timeout     = timeout,
            Sleep       = sleep
        };

        return WithRetryAsync(code, [rule], logger, cancellationToken);
    }

    /// <summary>
    /// Converts an asynchronous code block without a return value
    /// to one that returns a dummy value.
    /// </summary>
    private static Func<Task<bool>> Wrap
    (
        Func<Task> code
    )
    {
        ArgumentNullException.ThrowIfNull(code);

        return async () =>
        {
            await code().ConfigureAwait(false);
            return true;
        };
    }
}
