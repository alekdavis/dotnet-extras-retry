# DotNetExtras.Retry

`DotNetExtras.Retry` is a .NET Core library that allows applications to recover from and retry failed operations. This library is similar to [Polly](https://github.com/App-vNext/Polly), but it is much simpler because it only focuses only on three most common scenarios.

Use the `DotNetExtras.Retry` library to:

1. Retry an operation a specified number of times with an optional delay between attempts.
1. Retry an operation for the specified period of time with an optional delay between attempts.
1. Reload an object (configuration, settings, etc.) and retry an operation with the reloaded state (one or more times or for a period of time, with an optional delay between attempts).

The nice thing about the `DotNetExtras.Retry` library is that it is extremely easy to integrate with the existing code.

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
