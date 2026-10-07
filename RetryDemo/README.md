# DotNetExtras.RetryLibDemo project
This project implements a console application with the code samples illustrating how to use the `DotNetExtras.Retry` library APIs for retrying failed operation. The samples cover the most common scenarios:

1. Retrying an operation a number of times.
1. Retrying an operation within the specified timeout.
1. Retrying an operation after reloading the application configuration.
1. Retrying an operation based on a custom condition (predicate) that inspects the returned result instead of catching an exception.
1. Retrying an operation on either an exception or a custom condition (combined mode).
1. Retrying an operation on multiple conditions using retry rules, each with its own reload action, retry budget, and delay (e.g., reloading an expired client secret and backing off on HTTP 429 using the `Retry-After` value), with an optional total timeout.
1. Retrying an asynchronous operation with an asynchronous reload (`IReloadableAsync`) and a cancellation token.

For more details, read the source code comments.
