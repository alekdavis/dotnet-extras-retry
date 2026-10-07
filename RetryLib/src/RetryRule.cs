namespace DotNetExtras.Retry;

/// <summary>
/// Defines a single retry condition with its own reload action,
/// retry budget, and delay, for use with the rule-based
/// <c>Execute.WithRetry</c> and <c>Execute.WithRetryAsync</c> methods.
/// </summary>
/// <typeparam name="T">
/// Data type of the value returned by the code block.
/// </typeparam>
/// <remarks>
/// <para>
/// When the code block fails (throws an exception or returns a value),
/// the rules are evaluated in order and the first matching rule handles the failure.
/// Each rule tracks its own budget independently of the other rules.
/// </para>
/// <para>
/// If neither <see cref="Attempts"/> nor <see cref="Timeout"/> is set,
/// the rule allows two attempts (one retry).
/// If both are set, the rule is exhausted when either limit is reached.
/// </para>
/// <para>
/// When the matching rule is exhausted, the last failure wins:
/// an exception is rethrown and a returned value is returned.
/// A failure that does not match any rule is not retried.
/// </para>
/// </remarks>
public sealed class RetryRule<T>
{
    /// <summary>
    /// Predicate that returns <see langword="true"/> if the thrown exception
    /// must be handled by this rule.
    /// </summary>
    public Func<Exception, bool>? OnException { get; init; }

    /// <summary>
    /// Predicate that returns <see langword="true"/> if the returned value
    /// must be handled by this rule (i.e., the result is not acceptable).
    /// </summary>
    public Func<T, bool>? OnResult { get; init; }

    /// <summary>
    /// Object whose <see cref="IReloadable.Reload"/> method is called
    /// before each retry triggered by this rule.
    /// </summary>
    public IReloadable? Reload { get; init; }

    /// <summary>
    /// Object whose <see cref="IReloadableAsync.ReloadAsync"/> method is awaited
    /// before each retry triggered by this rule (asynchronous methods only).
    /// </summary>
    /// <remarks>
    /// Takes precedence over <see cref="Reload"/> in the asynchronous methods.
    /// The synchronous methods reject rules that specify only this property.
    /// </remarks>
    public IReloadableAsync? ReloadAsync { get; init; }

    /// <summary>
    /// Maximum number of tries attributed to this rule
    /// (the number of retries is one less).
    /// </summary>
    public int? Attempts { get; init; }

    /// <summary>
    /// Period of time, measured from the first failure matched by this rule,
    /// after which this rule stops retrying.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Fixed sleep time before each retry triggered by this rule.
    /// </summary>
    public TimeSpan? Sleep { get; init; }

    /// <summary>
    /// Function that calculates the sleep time before a retry
    /// from the returned value (if any) or the thrown exception (if any),
    /// such as a value from the <c>Retry-After</c> HTTP header.
    /// </summary>
    /// <remarks>
    /// If the function returns <see langword="null"/>,
    /// the <see cref="Sleep"/> value is used.
    /// </remarks>
    public Func<T?, Exception?, TimeSpan?>? GetSleep { get; init; }

    /// <summary>
    /// Checks if the exception matches this rule.
    /// </summary>
    internal bool Matches(Exception ex) => OnException != null && OnException(ex);

    /// <summary>
    /// Checks if the returned value matches this rule.
    /// </summary>
    internal bool Matches(T result) => OnResult != null && OnResult(result);

    /// <summary>
    /// Checks if this rule has no retries left.
    /// </summary>
    /// <param name="retries">
    /// Number of retries already performed for this rule.
    /// </param>
    /// <param name="firstMatchTime">
    /// Time (UTC) of the first failure matched by this rule.
    /// </param>
    internal bool IsExhausted(int retries, DateTime firstMatchTime)
    {
        if (Attempts == null && Timeout == null)
        {
            return retries >= 1;
        }

        return (Attempts != null && retries >= Attempts.Value - 1) ||
               (Timeout != null && DateTime.UtcNow - firstMatchTime > Timeout.Value);
    }

    /// <summary>
    /// Calculates the sleep time before the next retry.
    /// </summary>
    internal TimeSpan? GetSleepTime(T? result, Exception? ex) => GetSleep?.Invoke(result, ex) ?? Sleep;
}
