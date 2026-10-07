using System.Diagnostics;
using DotNetExtras.Retry;

namespace RetryTests;

public class SecretExpiredException: Exception
{
    public SecretExpiredException(): base("Client secret expired.") { }
}

public class CountingReloadable: IReloadable
{
    public int ReloadCount = 0;

    public Action? OnReload { get; set; }

    public void Reload()
    {
        ReloadCount++;
        OnReload?.Invoke();
    }
}

public class AsyncReloadable: IReloadable, IReloadableAsync
{
    public int ReloadCount = 0;
    public int SyncReloadCount = 0;

    public void Reload()
    {
        SyncReloadCount++;
    }

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        ReloadCount++;
    }
}

public class RetryRuleTests
{
    private const int TooManyRequests = 429;
    private const int Ok = 200;

    [Fact]
    public void ExecuteWithRetry_Sleep_WholeSeconds_Waits()
    {
        int calls = 0;
        Stopwatch stopwatch = Stopwatch.StartNew();

        Execute.WithRetry<InvalidOperationException>(() =>
        {
            if (++calls == 1)
            {
                throw new InvalidOperationException();
            }

        }, 2, TimeSpan.FromSeconds(1));

        stopwatch.Stop();

        Assert.Equal(2, calls);
        Assert.True(stopwatch.ElapsedMilliseconds >= 900, $"Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
    }

    [Fact]
    public void ExecuteWithRetry_Rules_IndependentBudgets_Success()
    {
        CountingReloadable secrets = new();
        int calls = 0;

        int result = Execute.WithRetry(() =>
        {
            calls++;

            return calls switch
            {
                1 => throw new SecretExpiredException(),
                2 or 3 => TooManyRequests,
                _ => Ok
            };
        },
        new RetryRule<int>
        {
            OnException = ex => ex is SecretExpiredException,
            Reload      = secrets,
            Attempts    = 2
        },
        new RetryRule<int>
        {
            OnResult = status => status == TooManyRequests,
            Attempts = 3
        });

        Assert.Equal(Ok, result);
        Assert.Equal(4, calls);
        Assert.Equal(1, secrets.ReloadCount);
    }

    [Fact]
    public void ExecuteWithRetry_Rules_ResultRuleExhausted_ReturnsLastResult()
    {
        int calls = 0;

        int result = Execute.WithRetry(() =>
        {
            calls++;
            return TooManyRequests;
        },
        new RetryRule<int>
        {
            OnResult = status => status == TooManyRequests,
            Attempts = 3
        });

        Assert.Equal(TooManyRequests, result);
        Assert.Equal(3, calls);
    }

    [Fact]
    public void ExecuteWithRetry_Rules_ExceptionRuleExhausted_Rethrows()
    {
        int calls = 0;

        Assert.Throws<SecretExpiredException>(() => Execute.WithRetry<int>(() =>
        {
            calls++;
            throw new SecretExpiredException();
        },
        new RetryRule<int>
        {
            OnException = ex => ex is SecretExpiredException,
            Attempts    = 2
        }));

        Assert.Equal(2, calls);
    }

    [Fact]
    public void ExecuteWithRetry_Rules_UnmatchedException_Propagates()
    {
        int calls = 0;

        Assert.Throws<ArgumentException>(() => Execute.WithRetry<int>(() =>
        {
            calls++;
            throw new ArgumentException();
        },
        new RetryRule<int>
        {
            OnException = ex => ex is InvalidOperationException,
            Attempts    = 5
        }));

        Assert.Equal(1, calls);
    }

    [Fact]
    public void ExecuteWithRetry_Rules_DynamicSleep_ReceivesResult()
    {
        List<int> observed = [];
        int calls = 0;

        int result = Execute.WithRetry(() =>
        {
            return ++calls < 3 ? TooManyRequests : Ok;
        },
        new RetryRule<int>
        {
            OnResult = status => status == TooManyRequests,
            Attempts = 5,
            GetSleep = (status, ex) =>
            {
                observed.Add(status);
                return TimeSpan.Zero;
            }
        });

        Assert.Equal(Ok, result);
        Assert.Equal([TooManyRequests, TooManyRequests], observed);
    }

    [Fact]
    public void ExecuteWithRetry_Rules_Empty_Throws()
    {
        Assert.Throws<ArgumentException>(() => Execute.WithRetry(() => Ok, Array.Empty<RetryRule<int>>()));
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_Exception_Reload_Success()
    {
        CountingReloadable secrets = new();
        int calls = 0;

        int result = await Execute.WithRetryAsync<SecretExpiredException, int>(async () =>
        {
            await Task.Yield();

            if (++calls == 1)
            {
                throw new SecretExpiredException();
            }

            return Ok;

        }, secrets, 2, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(Ok, result);
        Assert.Equal(2, calls);
        Assert.Equal(1, secrets.ReloadCount);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_Combined_LastFailureException_Rethrows()
    {
        int calls = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            Execute.WithRetryAsync<InvalidOperationException, int>(async () =>
            {
                await Task.Yield();

                return ++calls == 1 ? TooManyRequests : throw new InvalidOperationException();

            }, status => status == TooManyRequests, null, 2, cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_Rules_IndependentBudgets_Success()
    {
        CountingReloadable secrets = new();
        int calls = 0;

        int result = await Execute.WithRetryAsync(async () =>
        {
            await Task.Yield();
            calls++;

            return calls switch
            {
                1 => TooManyRequests,
                2 => throw new SecretExpiredException(),
                3 => TooManyRequests,
                _ => Ok
            };
        }, [
            new RetryRule<int>
            {
                OnException = ex => ex is SecretExpiredException,
                Reload      = secrets,
                Attempts    = 2
            },
            new RetryRule<int>
            {
                OnResult = status => status == TooManyRequests,
                Attempts = 3
            }
        ], cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(Ok, result);
        Assert.Equal(4, calls);
        Assert.Equal(1, secrets.ReloadCount);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_Rules_ReloadAsync_Awaited()
    {
        AsyncReloadable secrets = new();
        int calls = 0;

        int result = await Execute.WithRetryAsync(async () =>
        {
            await Task.Yield();
            return ++calls == 1 ? throw new SecretExpiredException() : Ok;
        }, [
            new RetryRule<int>
            {
                OnException = ex => ex is SecretExpiredException,
                ReloadAsync = secrets
            }
        ], cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(Ok, result);
        Assert.Equal(1, secrets.ReloadCount);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_Exception_CallerImplementsAsync_PrefersReloadAsync()
    {
        AsyncReloadable secrets = new();
        int calls = 0;

        int result = await Execute.WithRetryAsync<SecretExpiredException, int>(async () =>
        {
            await Task.Yield();
            return ++calls == 1 ? throw new SecretExpiredException() : Ok;

        }, secrets, 2, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(Ok, result);
        Assert.Equal(1, secrets.ReloadCount);
        Assert.Equal(0, secrets.SyncReloadCount);
    }

    [Fact]
    public void ExecuteWithRetry_Rules_ReloadAsyncOnly_Throws()
    {
        Assert.Throws<ArgumentException>(() => Execute.WithRetry(() => Ok,
            new RetryRule<int> { OnResult = s => true, ReloadAsync = new AsyncReloadable() }));
    }

    [Fact]
    public void ExecuteWithRetry_Rules_Timeout_MeasuredFromFirstMatch()
    {
        int calls = 0;

        // The first rule consumes ~300 ms; the second rule's 200 ms timeout
        // must start when it first matches, so it still gets to retry.
        int result = Execute.WithRetry(() =>
        {
            calls++;
            return calls switch
            {
                1 or 2 => 1,
                3 => TooManyRequests,
                _ => Ok
            };
        },
        new RetryRule<int> { OnResult = s => s == 1, Attempts = 3, Sleep = TimeSpan.FromMilliseconds(150) },
        new RetryRule<int> { OnResult = s => s == TooManyRequests, Timeout = TimeSpan.FromMilliseconds(200) });

        Assert.Equal(Ok, result);
        Assert.Equal(4, calls);
    }

    [Fact]
    public void ExecuteWithRetry_Rules_TotalTimeout_StopsRetrying()
    {
        int calls = 0;

        int result = Execute.WithRetry(() =>
        {
            calls++;
            return TooManyRequests;
        },
        [
            new RetryRule<int>
            {
                OnResult = s => s == TooManyRequests,
                Attempts = 100,
                Sleep    = TimeSpan.FromMilliseconds(50)
            }
        ], totalTimeout: TimeSpan.FromMilliseconds(120));

        Assert.Equal(TooManyRequests, result);
        Assert.InRange(calls, 2, 5);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_Cancellation_StopsRetrying()
    {
        using CancellationTokenSource cts = new();
        CountingReloadable reloadable = new() { OnReload = cts.Cancel };
        int calls = 0;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Execute.WithRetryAsync<InvalidOperationException, int>(() =>
            {
                calls++;
                throw new InvalidOperationException();

            }, reloadable, 5, TimeSpan.FromSeconds(5), cancellationToken: cts.Token));

        Assert.Equal(1, calls);
    }
}
