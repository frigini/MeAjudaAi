using System.Linq.Expressions;
using MeAjudaAi.Shared.Jobs;

namespace MeAjudaAi.Shared.Tests.Unit.Jobs;

public class NoOpBackgroundJobServiceTests
{
    private readonly NoOpBackgroundJobService _sut;

    public NoOpBackgroundJobServiceTests()
    {
        _sut = new NoOpBackgroundJobService();
    }

    #region EnqueueAsync<T> Tests

    [Fact]
    public async Task EnqueueAsync_WithGenericMethodCall_ShouldCompleteSuccessfully()
    {
        // Arrange
        Expression<Func<TestJobService, Task>> methodCall = service => service.ProcessJobAsync();

        // Act
        var act = () => _sut.EnqueueAsync(methodCall);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnqueueAsync_WithDelay_ShouldCompleteSuccessfully()
    {
        // Arrange
        Expression<Func<TestJobService, Task>> methodCall = service => service.ProcessJobAsync();
        var delay = TimeSpan.FromMinutes(5);

        // Act
        var act = () => _sut.EnqueueAsync(methodCall, delay);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnqueueAsync_WithNullDelay_ShouldCompleteSuccessfully()
    {
        // Arrange
        Expression<Func<TestJobService, Task>> methodCall = service => service.ProcessJobAsync();

        // Act
        var act = () => _sut.EnqueueAsync(methodCall, delay: null);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnqueueAsync_WithMethodParameters_ShouldCompleteSuccessfully()
    {
        // Arrange
        Expression<Func<TestJobService, Task>> methodCall = service => service.ProcessWithDataAsync("test", 123);

        // Act
        var act = () => _sut.EnqueueAsync(methodCall);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region EnqueueAsync (non-generic) Tests

    [Fact]
    public async Task EnqueueAsync_WithStaticMethodCall_ShouldCompleteSuccessfully()
    {
        // Arrange
        Expression<Func<Task>> methodCall = () => TestJobService.StaticProcessJobAsync();

        // Act
        var act = () => _sut.EnqueueAsync(methodCall);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnqueueAsync_NonGeneric_WithDelay_ShouldCompleteSuccessfully()
    {
        // Arrange
        Expression<Func<Task>> methodCall = () => TestJobService.StaticProcessJobAsync();
        var delay = TimeSpan.FromHours(1);

        // Act
        var act = () => _sut.EnqueueAsync(methodCall, delay);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnqueueAsync_NonGeneric_WithNullDelay_ShouldCompleteSuccessfully()
    {
        // Arrange
        Expression<Func<Task>> methodCall = () => TestJobService.StaticProcessJobAsync();

        // Act
        var act = () => _sut.EnqueueAsync(methodCall, delay: null);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region ScheduleRecurringAsync Tests

    [Fact]
    public async Task ScheduleRecurringAsync_WithCronExpression_ShouldCompleteSuccessfully()
    {
        // Arrange
        var jobId = "daily-cleanup";
        Expression<Func<Task>> methodCall = () => TestJobService.CleanupAsync();
        var cronExpression = "0 0 * * *"; // Daily at midnight

        // Act
        var act = () => _sut.ScheduleRecurringAsync(jobId, methodCall, cronExpression);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ScheduleRecurringAsync_WithHourlyCron_ShouldCompleteSuccessfully()
    {
        // Arrange
        var jobId = "hourly-sync";
        Expression<Func<Task>> methodCall = () => TestJobService.SyncDataAsync();
        var cronExpression = "0 * * * *"; // Every hour

        // Act
        var act = () => _sut.ScheduleRecurringAsync(jobId, methodCall, cronExpression);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ScheduleRecurringAsync_WithMinuteCron_ShouldCompleteSuccessfully()
    {
        // Arrange
        var jobId = "every-5-minutes";
        Expression<Func<Task>> methodCall = () => TestJobService.HealthCheckAsync();
        var cronExpression = "*/5 * * * *"; // Every 5 minutes

        // Act
        var act = () => _sut.ScheduleRecurringAsync(jobId, methodCall, cronExpression);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ScheduleRecurringAsync_MultipleJobs_ShouldCompleteSuccessfully()
    {
        // Arrange
        var job1 = "job-1";
        var job2 = "job-2";
        Expression<Func<Task>> methodCall1 = () => TestJobService.CleanupAsync();
        Expression<Func<Task>> methodCall2 = () => TestJobService.SyncDataAsync();

        // Act
        await _sut.ScheduleRecurringAsync(job1, methodCall1, "0 0 * * *");
        var act = () => _sut.ScheduleRecurringAsync(job2, methodCall2, "0 * * * *");

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region NoOp Behavior Verification Tests

    [Fact]
    public async Task EnqueueAsync_ShouldCompleteImmediatelyWithoutActualExecution()
    {
        // Arrange
        Expression<Func<TestJobService, Task>> methodCall = s => s.ProcessJobAsync();

        // Act
        var startTime = DateTime.UtcNow;
        await _sut.EnqueueAsync(methodCall);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert - Should complete immediately (< 100ms)
        elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task ScheduleRecurringAsync_ShouldCompleteImmediatelyWithoutActualExecution()
    {
        // Arrange
        Expression<Func<Task>> methodCall = () => TestJobService.StaticProcessJobAsync();

        // Act
        var startTime = DateTime.UtcNow;
        await _sut.ScheduleRecurringAsync("test-job", methodCall, "* * * * *");
        var elapsed = DateTime.UtcNow - startTime;

        // Assert - Should complete immediately (< 100ms)
        elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task MultipleEnqueues_ShouldAllCompleteImmediately()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            var task = _sut.EnqueueAsync<TestJobService>(s => s.ProcessJobAsync());
            tasks.Add(task);
        }

        // Assert
        await Task.WhenAll(tasks);
        tasks.Should().OnlyContain(t => t.IsCompletedSuccessfully);
    }

    #endregion

    #region Test Service Class

    private class TestJobService
    {
        public Task ProcessJobAsync() => Task.CompletedTask;

        public Task ProcessWithDataAsync(string data, int count) => Task.CompletedTask;

        public static Task StaticProcessJobAsync() => Task.CompletedTask;

        public static Task CleanupAsync() => Task.CompletedTask;

        public static Task SyncDataAsync() => Task.CompletedTask;

        public static Task HealthCheckAsync() => Task.CompletedTask;
    }

    #endregion
}
