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
    private readonly IBackendService _backendService = A.Fake<IBackendService>();

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

        Execute.WithRetry<InvalidOperationException>(service.DoSomething, service);

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
            Execute.WithRetry<InvalidOperationException>(service.DoSomething, service);
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
            Execute.WithRetry<ArgumentException>(service.DoSomething, service);
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

        int result = Execute.WithRetry<InvalidOperationException, int>(service.DoSomethingElse, service);

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
            result = Execute.WithRetry<InvalidOperationException, int>(service.DoSomethingElse, service);
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
            result = Execute.WithRetry<ArgumentException, int>(service.DoSomethingElse, service);
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

    [Fact]
    public void ExecuteWithRetry_Predicate_Attempts_Success()
    {
        Service service = new(_backendService);

        A.CallTo(() => _backendService.DoSomethingElse())
            .Returns<int>(0)
            .Once()
            .Then
            .Returns<int>(5);

        int result = Execute.WithRetry<int>(service.DoSomethingElse, result => result == 0, service, 3);

        Assert.Equal(5, result);
        Assert.Equal(1, service.ReloadCount);
        Assert.Equal(2, service.AttemptCount);
    }

    [Fact]
    public void ExecuteWithRetry_Predicate_Attempts_Exhausted()
    {
        Service service = new(_backendService);

        A.CallTo(() => _backendService.DoSomethingElse())
            .Returns<int>(0);

        int result = Execute.WithRetry<int>(service.DoSomethingElse, result => result == 0, service, 3);

        Assert.Equal(0, result);
        Assert.Equal(2, service.ReloadCount);
        Assert.Equal(3, service.AttemptCount);
    }

    [Fact]
    public void ExecuteWithRetry_Predicate_NoRetry()
    {
        Service service = new(_backendService);

        A.CallTo(() => _backendService.DoSomethingElse())
            .Returns<int>(5);

        int result = Execute.WithRetry<int>(service.DoSomethingElse, result => result == 0, service, 3);

        Assert.Equal(5, result);
        Assert.Equal(0, service.ReloadCount);
        Assert.Equal(1, service.AttemptCount);
    }

    [Fact]
    public void ExecuteWithRetry_Predicate_Timeout_Success()
    {
        Service service = new(_backendService);

        A.CallTo(() => _backendService.DoSomethingElse())
            .Returns<int>(0)
            .Once()
            .Then
            .Returns<int>(5);

        int result = Execute.WithRetry<int>(service.DoSomethingElse, result => result == 0, service, TimeSpan.FromSeconds(5));

        Assert.Equal(5, result);
        Assert.Equal(1, service.ReloadCount);
        Assert.Equal(2, service.AttemptCount);
    }

    [Fact]
    public void ExecuteWithRetry_Combined_PredicateTriggersRetry_Success()
    {
        Service service = new(_backendService);

        A.CallTo(() => _backendService.DoSomethingElse())
            .Returns<int>(0)
            .Once()
            .Then
            .Returns<int>(5);

        int result = Execute.WithRetry<InvalidOperationException, int>(service.DoSomethingElse, result => result == 0, service, 3);

        Assert.Equal(5, result);
        Assert.Equal(1, service.ReloadCount);
        Assert.Equal(2, service.AttemptCount);
    }

    [Fact]
    public void ExecuteWithRetry_Combined_ExceptionTriggersRetry_Success()
    {
        Service service = new(_backendService);

        A.CallTo(() => _backendService.DoSomethingElse())
            .Throws<InvalidOperationException>()
            .Once()
            .Then
            .Returns<int>(5);

        int result = Execute.WithRetry<InvalidOperationException, int>(service.DoSomethingElse, result => result == 0, service, 3);

        Assert.Equal(5, result);
        Assert.Equal(1, service.ReloadCount);
        Assert.Equal(2, service.AttemptCount);
    }

    [Fact]
    public void ExecuteWithRetry_Combined_LastFailureException_Rethrows()
    {
        Service service = new(_backendService);

        A.CallTo(() => _backendService.DoSomethingElse())
            .Returns<int>(0)
            .Once()
            .Then
            .Throws<InvalidOperationException>();

        int result = 0;
        Exception? exception = null;

        try
        {
            result = Execute.WithRetry<InvalidOperationException, int>(service.DoSomethingElse, result => result == 0, service, 2);
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
    public void ExecuteWithRetry_Combined_LastFailurePredicate_ReturnsResult()
    {
        Service service = new(_backendService);

        A.CallTo(() => _backendService.DoSomethingElse())
            .Throws<InvalidOperationException>()
            .Once()
            .Then
            .Returns<int>(0);

        int result = Execute.WithRetry<InvalidOperationException, int>(service.DoSomethingElse, result => result == 0, service, 2);

        Assert.Equal(0, result);
        Assert.Equal(1, service.ReloadCount);
        Assert.Equal(2, service.AttemptCount);
    }

    [Fact]
    public void ExecuteWithRetry_Combined_UnexpectedException_Propagates()
    {
        Service service = new(_backendService);

        A.CallTo(() => _backendService.DoSomethingElse())
            .Throws<InvalidOperationException>();

        Exception? exception = null;

        try
        {
            Execute.WithRetry<ArgumentException, int>(service.DoSomethingElse, result => result == 0, service, 3);
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
    public void ExecuteWithRetry_Combined_Timeout_ExceptionTriggersRetry_Success()
    {
        Service service = new(_backendService);

        A.CallTo(() => _backendService.DoSomethingElse())
            .Throws<InvalidOperationException>()
            .Once()
            .Then
            .Returns<int>(5);

        int result = Execute.WithRetry<InvalidOperationException, int>(service.DoSomethingElse, result => result == 0, service, TimeSpan.FromSeconds(5));

        Assert.Equal(5, result);
        Assert.Equal(1, service.ReloadCount);
        Assert.Equal(2, service.AttemptCount);
    }
}