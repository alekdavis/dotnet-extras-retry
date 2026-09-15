# DotNetExtras.Retry

`DotNetExtras.Retry` is a .NET Core library that allows applications to recover from and retry failed operations. This library is similar to [Polly](https://github.com/App-vNext/Polly), but it is much simpler because it only focuses on three most common scenarios.

Use the `DotNetExtras.Retry` library to:

1. Retry an operation a specified number of times with an optional delay between attempts.
1. Retry an operation for the specified period of time with an optional delay between attempts.
1. Reload an object (configuration, settings, etc.) and retry an operation with the reloaded state (one or more times or for a period of time, with an optional delay between attempts).
1. Retry an operation based on a custom condition (predicate) that inspects the returned result instead of catching an exception, or a combination of both an exception and a predicate.

The nice thing about the `DotNetExtras.Retry` library is that it is extremely easy to integrate with the existing code.

## Usage

The following example illustrates how to detect a failure that may be caused by an old configuration setting, reload the settings, and retry the operation.

```cs
using DotNetExtras.Retry;
...

// This class implements the Reload() method of the IReloadable interface,
// in which it reloads the configuration settings that could have changed.
ReloadableService service = new();

// If the operation throws a NotSupportedException,
// reload the service object and retry the operation one more time.
// The operation is expected to return an int value.
int result = Execute.WithRetry<NotSupportedException, int>(() => 
{
    // BEGINNING OF THE CODE BLOCK THAT WILL BE RETRIED.
    try
    {
        // Attempt to perform the operation.
        return service.DoSomething();
    }
    // This is not the expected exception for the retry,
    // but...
    catch (InvalidOperationException ex)
    {
        // ...we can simulate the expected exception for an appropriate condition.
        if (ex.Message.StartsWith("Unexpected"))
        {
            throw new NotSupportedException("Simulated exception triggering a reload.", ex);
        }

        // This will handle both the expected exception 
        // leading to a reload and retry (one retry attempt only),
        // as well as an unhandled exception that will 
        // result in error.
        throw;
    }
    // END OF THE CODE BLOCK THAT WILL BE RETRIED.

}, service);
// We are passing the same service object here because it is the one that 
// implements the reload method, but it can be a different object.
// We use the defaults for the delay (no delay) and the maximum attempts (2).
```

### Retry a fixed number of times

Retry a flaky operation up to a maximum number of attempts, pausing between attempts. If every attempt fails, the last exception is rethrown.

```cs
using DotNetExtras.Retry;
...

// Retry up to 3 times, waiting 500 milliseconds between attempts.
// Any exception triggers a retry (use the generic overload to narrow it down).
int value = Execute.WithRetry<int>(() =>
{
    return externalApi.GetValue();

}, attempts: 3, sleep: TimeSpan.FromMilliseconds(500));
```

### Retry for a period of time

Keep retrying an operation until it succeeds or the timeout elapses. This is handy when waiting for an eventually consistent backend (e.g., a record that is still being replicated).

```cs
using DotNetExtras.Retry;
...

// Retry only on the TimeoutException for up to 2 minutes,
// pausing 5 seconds between attempts.
User user = Execute.WithRetry<TimeoutException, User>(() =>
{
    return directory.GetUser(userId);

}, timeout: TimeSpan.FromMinutes(2), sleep: TimeSpan.FromSeconds(5));
```

### Retry based on a custom condition (predicate)

Sometimes a failed call does not throw; it returns a result that signals "not ready yet" (an HTTP `200` with a `Pending` status, an empty collection, etc.). Instead of faking an exception, pass a predicate that inspects the result and returns `true` to retry.

```cs
using DotNetExtras.Retry;
...

// Poll a long-running job until it is no longer "Running",
// retrying up to 10 times with a 1-second delay.
JobStatus status = Execute.WithRetry<JobStatus>(() =>
{
    return jobs.GetStatus(jobId);

}, retryCondition: result => result.State == "Running", attempts: 10, sleep: TimeSpan.FromSeconds(1));

// When the attempts are exhausted, the last result is returned
// (it may still satisfy the retry condition), so inspect it before using it.
```

### Retry on an exception OR a custom condition (combined mode)

Many real-world APIs signal transient trouble in two different ways at once: a hard failure surfaces as an exception, while a soft failure comes back as an unacceptable result. The combined overloads retry on either signal.

```cs
using DotNetExtras.Retry;
...

// Retry when the call throws a transient HttpRequestException OR
// when it returns a response that is still marked as throttled.
// Retry up to 5 times, reloading the service and pausing 2 seconds between attempts.
Response response = Execute.WithRetry<HttpRequestException, Response>(() =>
{
    return service.Send(request);

}, retryCondition: result => result.IsThrottled, service, attempts: 5, sleep: TimeSpan.FromSeconds(2));

// "Last failure wins" on exhaustion: if the final attempt threw the expected
// exception, it is rethrown; otherwise, the last returned value is returned.
```

You can find the complete example and other scenarios covered in the [demo application](https://github.com/alekdavis/dotnet-extras-retry/tree/main/RetryDemo).

## Documentation

For complete documentation, usage details, and code samples, see:

- [Documentation](https://alekdavis.github.io/dotnet-extras-retry)
- [Unit tests](https://github.com/alekdavis/dotnet-extras-retry/tree/main/RetryTests)
- [Demo](https://github.com/alekdavis/dotnet-extras-retry/tree/main/RetryDemo)

## Package

Install the latest version of the `DotNetExtras.Retry` NuGet package from:

- [https://www.nuget.org/packages/DotNetExtras.Retry](https://www.nuget.org/packages/DotNetExtras.Retry)

## See also

Check out other `DotNetExtras` libraries at:

- [https://github.com/alekdavis/dotnet-extras](https://github.com/alekdavis/dotnet-extras)
