using Microsoft.Extensions.Logging;

namespace DotNetExtras.Retry;

public static partial class Execute
{
    /// <summary>
    /// Executes code that returns a value and re-executes it according to
    /// the specified retry rules, each of which defines its own condition,
    /// reload action, retry budget, and delay.
    /// </summary>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Code block to be executed.
    /// </param>
    /// <param name="rules">
    /// Retry rules evaluated in order; the first matching rule handles a failure.
    /// </param>
    /// <returns>
    /// Value returned by the code block.
    /// </returns>
    /// <remarks>
    /// See <see cref="RetryRule{T}"/> for the rule semantics.
    /// </remarks>
    public static T WithRetry<T>
    (
        Func<T> code,
        params RetryRule<T>[] rules
    )
    {
        return WithRetry(code, (IEnumerable<RetryRule<T>>)rules);
    }

    /// <summary>
    /// Executes code that returns a value and re-executes it according to
    /// the specified retry rules, each of which defines its own condition,
    /// reload action, retry budget, and delay.
    /// </summary>
    /// <typeparam name="T">
    /// Data type of the value returned by the code block.
    /// </typeparam>
    /// <param name="code">
    /// Code block to be executed.
    /// </param>
    /// <param name="rules">
    /// Retry rules evaluated in order; the first matching rule handles a failure.
    /// </param>
    /// <param name="logger">
    /// Logs retry event information.
    /// </param>
    /// <param name="totalTimeout">
    /// Optional cap on the total time (measured from the first attempt)
    /// after which no rule can trigger another retry.
    /// </param>
    /// <returns>
    /// Value returned by the code block.
    /// </returns>
    /// <remarks>
    /// See <see cref="RetryRule{T}"/> for the rule semantics.
    /// </remarks>
    public static T WithRetry<T>
    (
        Func<T> code,
        IEnumerable<RetryRule<T>> rules,
        ILogger? logger = null,
        TimeSpan? totalTimeout = null
    )
    {
        ArgumentNullException.ThrowIfNull(code);
        RetryRule<T>[] ruleList = ValidateRules(rules);

        if (ruleList.Any(r => r.ReloadAsync != null && r.Reload == null))
        {
            throw new ArgumentException(
                "Synchronous retries require the Reload property; ReloadAsync is only supported by WithRetryAsync.", 
                nameof(rules));
        }

        int[] retries = new int[ruleList.Length];
        DateTime?[] firstMatch = new DateTime?[ruleList.Length];
        DateTime startTime = DateTime.UtcNow;

        bool IsExhausted(int index)
        {
            firstMatch[index] ??= DateTime.UtcNow;

            return (totalTimeout != null && DateTime.UtcNow - startTime > totalTimeout.Value) ||
                   ruleList[index].IsExhausted(retries[index], firstMatch[index]!.Value);
        }

        while (true)
        {
            T result;

            try
            {
                result = code();
            }
            catch (Exception ex)
            {
                int index = Array.FindIndex(ruleList, r => r.Matches(ex));

                if (index < 0 || IsExhausted(index))
                {
                    throw;
                }

                retries[index]++;

                RetryRule<T> rule = ruleList[index];
                Prepare(ex.GetType(), rule.GetSleepTime(default, ex), rule.Reload, logger);

                continue;
            }

            int resultIndex = Array.FindIndex(ruleList, r => r.Matches(result));

            if (resultIndex < 0 || IsExhausted(resultIndex))
            {
                return result;
            }

            retries[resultIndex]++;

            RetryRule<T> resultRule = ruleList[resultIndex];
            Prepare(resultRule.GetSleepTime(result, null), resultRule.Reload, logger);
        }
    }

    /// <summary>
    /// Validates the retry rules and converts them to an array.
    /// </summary>
    private static RetryRule<T>[] ValidateRules<T>
    (
        IEnumerable<RetryRule<T>> rules
    )
    {
        ArgumentNullException.ThrowIfNull(rules);

        RetryRule<T>[] ruleList = rules.ToArray();

        if (ruleList.Length == 0)
        {
            throw new ArgumentException("At least one retry rule must be specified.", nameof(rules));
        }

        if (ruleList.Any(r => r == null))
        {
            throw new ArgumentException("Retry rules cannot be null.", nameof(rules));
        }

        return ruleList;
    }
}
