# DotNetExtras.Retry

`DotNetExtras.Retry` is a .NET Core library that allows applications to recover from and retry failed operations. This library is similar to [Polly](https://github.com/App-vNext/Polly), but it is much simpler because it only focuses only on three most common scenarios.

Use the `DotNetExtras.Retry` library to:

1. Retry an operation a specified number of times with an optional delay between attempts.
1. Retry an operation for the specified period of time with an optional delay between attempts.
1. Reload an object (configuration, settings, etc.) and retry an operation with the reloaded state (one or more times or for a period of time, with an optional delay between attempts).

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

You can find the complete example and other scenarios covered in the [demo application](https://github.com/alekdavis/dotnet-extras-retry/tree/main/RetryDemo).

## Documentation

For complete documentation, usage details, and code samples, see:

- [Documentation](https://alekdavis.github.io/dotnet-extras-retry)
- [Demo](https://github.com/alekdavis/dotnet-extras-retry/tree/main/RetryDemo)

## Package

Install the latest version of the `DotNetExtras.Retry` Nuget package from:

- [https://www.nuget.org/packages/DotNetExtras.Retry](https://www.nuget.org/packages/DotNetExtras.Retry)

## See also

Check out other `DotNetExtras` libraries at:

- [https://github.com/alekdavis/dotnet-extras](https://github.com/alekdavis/dotnet-extras)
