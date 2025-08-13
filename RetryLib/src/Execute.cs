using Microsoft.Extensions.Logging;

namespace DotNetExtras.Retry;

/// <summary>
/// Implements easy to use retry logic for arbitrary code blocks 
/// allowing code to repeat a failed operation.
/// </summary>
/// 
/// <example>
/// <para>
/// EXAMPLE 1: Retry a failed operation a number of times.
/// </para>
/// <para>
/// A typical use case would be retrying to calling external API with a history of 
/// occasional failures that can be resolved by retrying the operation.
/// </para>
/// <code lang="csharp">
/// // This object makes an external API call.
/// Service service = new();
/// 
/// // Gets the value from the called method.
/// int result;
/// 
/// try
/// {
///     // Trigger retry only if an external service exception is detected.
///     // The catch block has custom logic determining if the condition is met
///     // in case the called method does not throw the expected exception.
///     result = Execute.WithRetry&lt;ExternalServiceException, int&gt;(() =&gt; 
///     {
///         try
///         {
///             // Perform an operation that may throw an exception.
///             return service.DoSomething();
///         }
///         catch (Exception ex)
///         {
///             // Determine if a retry is appropriate and if so,
///             // throw the expected exception.
///             if (ex is TimeoutException || ex is SomeOtherExpectedException)
///             {
///                 // Trigger the appropriate exception, 
///                 // but do not discard the original exception. 
///                 throw new ExternalServiceException("External exception detected.", ex);
///             }
///             
///             // Rethrow the original exception.
///             // If the original exception is not the expected one,
///             // a retry will not be attempted.
///             throw;
///         }
///         
///         // We will make 3 attempts with a half second delay between attempts.
///     }, 3, TimeSpan.FromMilliseconds(500));
/// 
///     // If we got so far, either the operation was successful or 
///     // the reload and retry worked, so it is okay to use the result.
///     Console.WriteLine($"Result: {result}");
/// }
/// catch
/// {
///     // If we got here, either the reload and retry did not help or 
///     // the condition for the reload and retry was not met.
/// }
/// </code>
/// <para>
/// EXAMPLE 2: Retry a failed operation for a period of time.
/// </para>
/// <para>
/// A typical use case here would be calling an external API 
/// that may be waiting for a background process to complete.
/// For example, when creating a new user in a directory (Azure, Active Directory),
/// it may take a few seconds (or minutes) to synchronize the record across backend systems,
/// so if a client creates a user and immediately follows up with an update,
/// it may encounter the user not found exception. 
/// In this case, attempting to repeat the update operation for a period of time may resolve the issue.
/// </para>
/// <code lang="csharp">
/// // This object makes an external API call.
/// Service service = new();
/// 
/// // Gets the value from the called method.
/// int result;
/// 
/// try
/// {
///     // Trigger retry only if an external service exception is detected.
///     // The catch block has custom logic determining if the condition is met
///     // in case the called method does not throw the expected exception.
///     result = Execute.WithRetry&lt;NotFoundException, int&gt;(() =&gt; 
///     {
///         try
///         {
///             // Perform an operation that may throw an exception.
///             return service.DoSomething();
///         }
///         catch (Exception ex)
///         {
///             // Determine if a retry is appropriate and if so,
///             // throw the expected exception.
///             if (ex is InvalidInputException || ex is SomeOtherExpectedException)
///             {
///                 // Trigger the appropriate exception, 
///                 // but do not discard the original exception. 
///                 throw new NotFoundException("Pending sync exception detected.", ex);
///             }
///             
///             // Rethrow the original exception.
///             // If the original exception is not the expected one,
///             // a retry will not be attempted.
///             throw;
///         }
///         
///         // We will keep trying for a couple of minutes
///         // with a ten second delay between attempts.
///     }, 3, TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(10));
/// 
///     // If we got so far, either the operation was successful or 
///     // the reload and retry worked, so it is okay to use the result.
///     Console.WriteLine($"Result: {result}");
/// }
/// catch
/// {
///     // If we got here, either the reload and retry did not help or 
///     // the condition for the reload and retry was not met.
/// }
/// </code> 
/// <para>
/// EXAMPLE 3: Check if the operation failed because of a bad credential,
/// reload the class with the updated credential, and retry the operation.
/// </para>
/// <para>
/// A typical use case would be updating a client secret used to call an external API
/// and letting the code refresh the settings with the updated credential before
/// retrying the operation.
/// </para>
/// <code lang="csharp">
/// // This object implements both the IReloadable.Reload() and the called methods 
/// // (if needed, the two methods can be implemented by different classes).
/// ReloadableService service = new();
/// 
/// // Gets the value from the called method.
/// int result;
/// 
/// try
/// {
///     // If the code block throws an invalid credentials exception,
///     // call the reload method and re-execute the code block.
///     // The catch block has custom logic determining if the condition is met
///     // in case the called method does not throw the expected exception.
///     result = Execute.WithRetry&lt;InvalidCredentialsException, int&gt;(() =&gt; 
///     {
///         try
///         {
///             // Perform an operation that may throw an exception.
///             return service.DoSomething();
///         }
///         catch (Exception ex)
///         {
///             // Determine if reload and retry is appropriate and 
///             // if so throw the expected exception.
///             if (ex is AccessDeniedException || ex is AuthenticationException)
///             {
///                 // Trigger the appropriate exception, 
///                 // but do not discard the original exception. 
///                 throw new InvalidCredentialsException(
///                     "Suspected expired client secret.", 
///                     ex);
///             }
///             
///             // Rethrow the original exception.
///             // If the original exception is not the expected one,
///             // a retry will not be attempted.
///             throw;
///         }
///         
///         // In this example, we assume that the same object implements the
///         // called operation and the reload method, but it doe not have
///         // to be the case (the two methods can be implemented by two classes).
///     }, service);
/// 
///     // If we got so far, either the operation was successful or 
///     // the reload and retry worked, so it is okay to use the result.
///     Console.WriteLine($"Result: {result}");
/// }
/// catch
/// {
///     // If we got here, either the reload and retry did not help or 
///     // the condition for the reload and retry was not met.
/// }
/// </code>
/// </example>
public static partial class Execute
{
    /// <summary>
    /// Implements common logic for the retry methods
    /// </summary>
    /// <param name="exceptionType">
    /// Type of exception triggering a retry.
    /// </param>
    /// <param name="sleep">
    /// Wait time before a retry.
    /// </param>
    /// <param name="caller">
    /// Service that mus be reloaded before a retry.
    /// </param>
    /// <param name="logger">
    /// Logs retry event information.
    /// </param>
    private static void Prepare
    (
        Type exceptionType,
        TimeSpan? sleep = null,
        IReloadable? caller = null,
        ILogger? logger = null
    )
    {
        logger?.LogInformation("Preparing to retry the operation after '{exception:l}' was caught.", 
            exceptionType.Name);

        if (caller != null)
        {
            logger?.LogInformation("Reloading '{caller:l}' instance.", caller.GetType().Name);

            caller.Reload();
        }

        int waitMilliseconds =  sleep?.Milliseconds ?? 0;

        if (waitMilliseconds > 0)
        {
            logger?.LogInformation("Waiting {waitMilliseconds} milliseconds before retrying.", 
                waitMilliseconds);

            Thread.Sleep(waitMilliseconds);
        }

        logger?.LogInformation("Retrying the operation.");
    }
}
