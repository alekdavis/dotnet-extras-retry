using Microsoft.Extensions.Logging;

namespace DotNetExtras.Retry;

public static partial class Execute
{
    /// <summary>
    /// Executes code that does not return a value and, 
    /// if the code throws any exception,
    /// calls the <see cref="IReloadable.Reload"/> method 
    /// and re-executes the same code again.
    /// </summary>
    /// <param name="code">
    /// Code block to be executed.
    /// </param>
    /// <param name="caller">
    /// Object implementing the <see cref="IReloadable.Reload"/> method
    /// that will be called on a retry.
    /// </param>
    /// <param name="sleep">
    /// Defines sleep time before object reloading and retrying the code re-execution.
    /// </param>
    /// <param name="logger">
    /// Logs reload and retry event information.
    /// </param>
    public static void WithRetry
    (
        Action code,
        IReloadable? caller,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    {
        WithRetry<Exception>(code, caller, sleep, logger);
    }

    /// <summary>
    /// Executes code that does not return a value and, 
    /// if the code throws a specific exception,
    /// calls the <see cref="IReloadable.Reload"/> method 
    /// and re-executes the same code again.
    /// </summary>
    /// <typeparam name="E">
    /// Exception type that triggers a reload and code re-execution.
    /// </typeparam>
    /// <param name="code">
    /// Code block to be executed.
    /// </param>
    /// <param name="caller">
    /// Object implementing the <see cref="IReloadable.Reload"/> method
    /// that will be called on a retry.
    /// </param>
    /// <param name="sleep">
    /// Defines sleep time before object reloading and retrying the code re-execution.
    /// </param>
    /// <param name="logger">
    /// Logs reload and retry event information.
    /// </param>
    public static void WithRetry<E>
    (
        Action code,
        IReloadable? caller,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    where E: Exception
    {
        WithRetry<E>(code, caller, 2, sleep, logger);
    }

    /// <summary>
    /// Executes code that returns a value and, 
    /// if the code throws a specific exception,
    /// calls the <see cref="IReloadable.Reload"/> method 
    /// and re-executes the same code again.
    /// </summary>
    /// <typeparam name="E">
    /// Exception type that triggers a reload and code re-execution.
    /// </typeparam>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Code block to be executed.
    /// </param>
    /// <param name="caller">
    /// Object implementing the <see cref="IReloadable.Reload"/> method
    /// that will be called on a retry.
    /// </param>
    /// <param name="sleep">
    /// Defines sleep time before object reloading and retrying the code re-execution.
    /// </param>
    /// <param name="logger">
    /// Logs reload and retry event information.
    /// </param>
    /// <returns>
    /// Value returned by the code block.
    /// </returns>
    public static T WithRetry<E,T>
    (
        Func<T> code,
        IReloadable caller,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    where E: Exception
    {
        return WithRetry<E, T>(code, caller, 2, sleep, logger);
    }

    /// <summary>
    /// Executes code that does not return a value and, 
    /// if the code throws any exception,
    /// re-executes it until it runs out of attempts.
    /// </summary>
    /// <param name="code">
    /// Code block to be executed.
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
    public static void WithRetry
    (
        Action code,
        int attempts,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    {
        WithRetry<Exception>(code, attempts, sleep, logger);
    }

    /// <summary>
    /// Executes code that does not return a value and, 
    /// if the code throws any exception,
    /// calls the <see cref="IReloadable.Reload"/> method and
    /// re-executes it until it runs out of attempts.
    /// </summary>
    /// <param name="code">
    /// Code block to be executed.
    /// </param>
    /// <param name="caller">
    /// Object implementing the <see cref="IReloadable.Reload"/> method
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
    public static void WithRetry
    (
        Action code,
        IReloadable caller,
        int attempts,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    {
        WithRetry<Exception>(code, caller, attempts, sleep, logger);
    }

    /// <summary>
    /// Executes code that does not return a value and, 
    /// if the code throws a specific exception,
    /// re-executes it until it runs out of attempts.
    /// </summary>
    /// <typeparam name="E">
    /// Exception type that triggers code re-execution.
    /// </typeparam>
    /// <param name="code">
    /// Code block to be executed.
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
    public static void WithRetry<E>
    (
        Action code,
        int attempts,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    where E : Exception
    {
        WithRetry<E>(code, null, attempts, sleep, logger);
    }

    /// <summary>
    /// Executes code that does not return a value and, 
    /// if the code throws a specific exception,
    /// calls the <see cref="IReloadable.Reload"/> method and
    /// re-executes it until it runs out of attempts.
    /// </summary>
    /// <typeparam name="E">
    /// Exception type that triggers code re-execution.
    /// </typeparam>
    /// <param name="code">
    /// Code block to be executed.
    /// </param>
    /// <param name="caller">
    /// Object implementing the <see cref="IReloadable.Reload"/> method
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
    public static void WithRetry<E>
    (
        Action code,
        IReloadable? caller,
        int attempts,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    where E : Exception
    {
        while (true)
        {
            try
            {
                code();
                break;
            }
            catch (E)
            {
                if (--attempts <= 0)
                {
                    throw;
                }

                Prepare(typeof(E), sleep, caller, logger);
            }
        }
    }

    /// <summary>
    /// Executes code that returns a value and, 
    /// if the code throws any exception,
    /// re-executes it until it runs out of attempts.
    /// </summary>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Code block to be executed.
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
    /// <returns>
    /// Value returned by the code block.
    /// </returns>
    public static T WithRetry<T>
    (
        Func<T> code,
        int attempts,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    {
        return WithRetry<Exception, T>(code, attempts, sleep, logger);
    }

    /// <summary>
    /// Executes code that returns a value and, 
    /// if the code throws any exception,
    /// re-executes it until it runs out of attempts.
    /// </summary>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Code block to be executed.
    /// </param>
    /// <param name="caller">
    /// Object implementing the <see cref="IReloadable.Reload"/> method
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
    /// <returns>
    /// Value returned by the code block.
    /// </returns>
    public static T WithRetry<T>
    (
        Func<T> code,
        IReloadable? caller,
        int attempts,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    {
        return WithRetry<Exception, T>(code, caller, attempts, sleep, logger);
    }

    /// <summary>
    /// Executes code that returns a value and, 
    /// if the code throws a specific exception,
    /// re-executes it until it runs out of attempts.
    /// </summary>
    /// <typeparam name="E">
    /// Exception type that triggers code re-execution.
    /// </typeparam>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Code block to be executed.
    /// </param>
    /// <param name="attempts">
    /// Maximum number of tries.
    /// </param>
    /// <param name="sleep">
    /// Sleep time between retries.
    /// </param>
    /// <returns>
    /// Value returned by the code block.
    /// </returns>
    /// <param name="logger">
    /// Logs retry event information.
    /// </param>
    public static T WithRetry<E,T>
    (
        Func<T> code,
        int attempts,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    where E : Exception
    {
        return WithRetry<E, T>(code, null, attempts, sleep, logger);
    }

    /// <summary>
    /// Executes code that returns a value and, 
    /// if the code throws a specific exception,
    /// calls the <see cref="IReloadable.Reload"/> method and
    /// re-executes it until it runs out of attempts.
    /// </summary>
    /// <typeparam name="E">
    /// Exception type that triggers code re-execution.
    /// </typeparam>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Code block to be executed.
    /// </param>
    /// <param name="caller">
    /// Object implementing the <see cref="IReloadable.Reload"/> method
    /// that will be called on each retry.
    /// </param>
    /// <param name="attempts">
    /// Maximum number of tries.
    /// </param>
    /// <param name="sleep">
    /// Sleep time between retries.
    /// </param>
    /// <returns>
    /// Value returned by the code block.
    /// </returns>
    /// <param name="logger">
    /// Logs retry event information.
    /// </param>
    public static T WithRetry<E,T>
    (
        Func<T> code,
        IReloadable? caller,
        int attempts,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    where E : Exception
    {
        while (true)
        {
            try
            {
                return code();
            }
            catch (E)
            {
                if (--attempts <= 0)
                {
                    throw;
                }

                Prepare(typeof(E), sleep, caller, logger);
            }
        }
    }

    /// <summary>
    /// Executes code that does not return a value and, 
    /// if the code throws any exception,
    /// re-executes it until it runs out of time.
    /// </summary>
    /// <param name="code">
    /// Code block to be executed.
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
    public static void WithRetry
    (
        Action code,
        TimeSpan timeout,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    {
        WithRetry<Exception>(code, null, timeout, sleep, logger);
    }

    /// <summary>
    /// Executes code that does not return a value and, 
    /// if the code throws any exception,
    /// calls the <see cref="IReloadable.Reload"/> method and
    /// re-executes it until it runs out of time.
    /// </summary>
    /// <param name="code">
    /// Code block to be executed.
    /// </param>
    /// <param name="caller">
    /// Object implementing the <see cref="IReloadable.Reload"/> method
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
    public static void WithRetry
    (
        Action code,
        IReloadable? caller,
        TimeSpan timeout,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    {
        WithRetry<Exception>(code, caller, timeout, sleep, logger);
    }

    /// <summary>
    /// Executes code that does not return a value and, 
    /// if the code throws a specific exception,
    /// re-executes it until it runs out of time.
    /// </summary>
    /// <typeparam name="E">
    /// Exception type that triggers code re-execution.
    /// </typeparam>
    /// <param name="code">
    /// Code block to be executed.
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
    public static void WithRetry<E>
    (
        Action code,
        TimeSpan timeout,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    where E : Exception
    {
        WithRetry<E>(code, null, timeout, sleep, logger);
    }

    /// <summary>
    /// Executes code that does not return a value and, 
    /// if the code throws a specific exception,
    /// calls the <see cref="IReloadable.Reload"/> method and
    /// re-executes it until it runs out of time.
    /// </summary>
    /// <typeparam name="E">
    /// Exception type that triggers code re-execution.
    /// </typeparam>
    /// <param name="code">
    /// Code block to be executed.
    /// </param>
    /// <param name="caller">
    /// Object implementing the <see cref="IReloadable.Reload"/> method
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
    public static void WithRetry<E>
    (
        Action code,
        IReloadable? caller,
        TimeSpan timeout,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    where E : Exception
    {
        DateTime endTime = DateTime.UtcNow.Add(timeout);

        while (true)
        {
            try
            {
                code();
                break;
            }
            catch (E)
            {
                if (DateTime.UtcNow > endTime)
                {
                    throw;
                }

                Prepare(typeof(E), sleep, caller, logger);
            }
        }
    }

    /// <summary>
    /// Executes code that returns a value and, 
    /// if the code throws any exception,
    /// re-executes it until it runs out of time.
    /// </summary>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Code block to be executed.
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
    /// <returns>
    /// Value returned by the code block.
    /// </returns>
    public static T WithRetry<T>
    (
        Func<T> code,
        TimeSpan timeout,
        TimeSpan? sleep =  null,
        ILogger? logger = null
    )
    {
        DateTime endTime = DateTime.UtcNow.Add(timeout);

        while (true)
        {
            try
            {
                return code();
            }
            catch (Exception ex)
            {
                if (DateTime.UtcNow > endTime)
                {
                    throw;
                }

                Prepare(ex.GetType(), sleep, null, logger);
            }
        }
    }

    /// <summary>
    /// Executes code that returns a value and, 
    /// if the code throws any exception,
    /// re-executes it until it runs out of time.
    /// </summary>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Code block to be executed.
    /// </param>
    /// <param name="caller">
    /// Object implementing the <see cref="IReloadable.Reload"/> method
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
    /// <returns>
    /// Value returned by the code block.
    /// </returns>
    public static T WithRetry<T>
    (
        Func<T> code,
        IReloadable? caller,
        TimeSpan timeout,
        TimeSpan? sleep =  null,
        ILogger? logger = null
    )
    {
        return WithRetry<Exception, T>(code, caller, timeout, sleep, logger);
    }

    /// <summary>
    /// Executes code that returns a value and, 
    /// if the code throws a specific exception,
    /// re-executes it until it runs out of time.
    /// </summary>
    /// <typeparam name="E">
    /// Exception type that triggers code re-execution.
    /// </typeparam>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Code block to be executed.
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
    /// <returns>
    /// Value returned by the code block.
    /// </returns>
    public static T WithRetry<E,T>
    (
        Func<T> code,
        TimeSpan timeout,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    where E : Exception
    {
        return WithRetry<E, T>(code, null, timeout, sleep, logger);
    }

    /// <summary>
    /// Executes code that returns a value and, 
    /// if the code throws a specific exception,
    /// calls the <see cref="IReloadable.Reload"/> method and
    /// re-executes it until it runs out of time.
    /// </summary>
    /// <typeparam name="E">
    /// Exception type that triggers code re-execution.
    /// </typeparam>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Code block to be executed.
    /// </param>
    /// <param name="caller">
    /// Object implementing the <see cref="IReloadable.Reload"/> method
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
    /// <returns>
    /// Value returned by the code block.
    /// </returns>
    public static T WithRetry<E,T>
    (
        Func<T> code,
        IReloadable? caller,
        TimeSpan timeout,
        TimeSpan? sleep = null,
        ILogger? logger = null
    )
    where E : Exception
    {
        DateTime endTime = DateTime.UtcNow.Add(timeout);

        while (true)
        {
            try
            {
                return code();
            }
            catch (E)
            {
                if (DateTime.UtcNow > endTime)
                {
                    throw;
                }

                Prepare(typeof(E), sleep, caller, logger);
            }
        }
    }
}
