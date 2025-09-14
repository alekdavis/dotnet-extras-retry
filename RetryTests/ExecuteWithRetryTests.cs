using DotNetExtras.Retry;
using FakeItEasy;

namespace RetryTests;

public interface IBackendService
{
    void DoSomething();

    int DoSomethingElse();
}

public class BackendService: IBackendService
{
    public void DoSomething()
    {
        return;
    }

    public int DoSomethingElse()
    {
        return 1;
    }
}

public class Service: IReloadable
{
    private readonly IBackendService _backendService;

    public int ReloadCount = 0;
    public int AttemptCount = 0;

    public Service(IBackendService backendService)
    {
        _backendService = backendService;
    }

    public void Reload()
    {
        ReloadCount++;
    }

    public void DoSomething()
    {
        AttemptCount++;
        _backendService.DoSomething();
    }

    public int DoSomethingElse()
    {
        AttemptCount++;
        return _backendService.DoSomethingElse();
    }
}

public class ExecuteWithRetryTest
{
    private IBackendService _backendService = A.Fake<IBackendService>();

    public ExecuteWithRetryTest()
    {
    }

    [Fact]
    public void ExecuteWithRetry_DoSomething_Reload_Success()
    {
        Service service = new(_backendService);

        A.CallTo(() => _backendService.DoSomething())
            .Throws<InvalidOperationException>()
            .Once()
            .Then
            .DoesNothing();

        Execute.WithRetry<InvalidOperationException>(() =>
        {
            service.DoSomething();

        }, service);

        Assert.Equal(1, service.ReloadCount);
        Assert.Equal(2, service.AttemptCount);
    }

    [Fact]
    public void ExecuteWithRetry_DoSomething_Reload_Error()
    {
        Service service = new(_backendService);

        A.CallTo(() => _backendService.DoSomething())
            .Throws<InvalidOperationException>()
            .NumberOfTimes(2)
            .Then
            .DoesNothing();

        Exception? exception = null;

        try
        {
            Execute.WithRetry<InvalidOperationException>(() =>
            {
                service.DoSomething();

            }, service);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(1, service.ReloadCount);
        Assert.Equal(2, service.AttemptCount);
    }


    [Fact]
    public void ExecuteWithRetry_DoSomething_Reload_Error_WrongCondition()
    {
        Service service = new(_backendService);

        A.CallTo(() => _backendService.DoSomething())
            .Throws<InvalidOperationException>()
            .Once()
            .Then
            .DoesNothing();

        Exception? exception = null;

        try
        {
            Execute.WithRetry<ArgumentException>(() =>
            {
                service.DoSomething();

            }, service);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(0, service.ReloadCount);
        Assert.Equal(1, service.AttemptCount);
    }

    [Fact]
    public void ExecuteWithRetry_DoSomethingElse_Reload_Success()
    {
        Service service = new(_backendService);

        A.CallTo(() => _backendService.DoSomethingElse())
            .Throws<InvalidOperationException>()
            .Once()
            .Then
            .Returns<int>(5);

        int result = Execute.WithRetry<InvalidOperationException, int>(() =>
        {
            return service.DoSomethingElse();

        }, service);

        Assert.Equal(5, result);
        Assert.Equal(1, service.ReloadCount);
        Assert.Equal(2, service.AttemptCount);
    }

    [Fact]
    public void ExecuteWithRetry_DoSomethingElse_Reload_Error()
    {
        Service service = new(_backendService);

        A.CallTo(() => _backendService.DoSomethingElse())
            .Throws<InvalidOperationException>()
            .NumberOfTimes(2)
            .Then
            .Returns<int>(5);

        int result = 0;
        Exception? exception = null;

        try
        {
            result = Execute.WithRetry<InvalidOperationException, int>(() =>
            {
                return service.DoSomethingElse();

            }, service);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        Assert.Equal(0, result);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(1, service.ReloadCount);
        Assert.Equal(2, service.AttemptCount);
    }

    [Fact]
    public void ExecuteWithRetry_DoSomethingElse_Reload_Error_WrongCondition()
    {
        Service service = new(_backendService);

        A.CallTo(() => _backendService.DoSomethingElse())
            .Throws<InvalidOperationException>()
            .Once()
            .Then
            .Returns<int>(5);

        int result = 0;
        Exception? exception = null;

        try
        {
            result = Execute.WithRetry<ArgumentException, int>(() =>
            {
                return service.DoSomethingElse();

            }, service);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        Assert.Equal(0, result);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(0, service.ReloadCount);
        Assert.Equal(1, service.AttemptCount);
    }
}