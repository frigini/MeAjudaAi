using FluentAssertions;
using MeAjudaAi.Shared.Jobs;

namespace MeAjudaAi.Shared.Tests.Infrastructure;

[Trait("Category", "Integration")]
public class BackgroundJobServiceIntegrationTests
{
    #region NoOpBackgroundJobService Tests

    [Fact]
    public async Task NoOpBackgroundJobService_EnqueueAsync_WithMethod_ShouldComplete()
    {
        // Arrange
        var service = new NoOpBackgroundJobService();
        var testService = new TestService();

        // Act
        var act = async () => await service.EnqueueAsync<TestService>(s => s.DoWorkAsync());

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NoOpBackgroundJobService_EnqueueAsync_WithDelay_ShouldComplete()
    {
        // Arrange
        var service = new NoOpBackgroundJobService();
        var testService = new TestService();
        var delay = TimeSpan.FromMinutes(5);

        // Act
        var act = async () => await service.EnqueueAsync<TestService>(s => s.DoWorkAsync(), delay);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NoOpBackgroundJobService_EnqueueAsync_WithoutType_ShouldComplete()
    {
        // Arrange
        var service = new NoOpBackgroundJobService();

        // Act
        var act = async () => await service.EnqueueAsync(() => Task.CompletedTask);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NoOpBackgroundJobService_ScheduleRecurringAsync_ShouldComplete()
    {
        // Arrange
        var service = new NoOpBackgroundJobService();
        var jobId = "test-job";
        var cronExpression = "0 0 * * *"; // Daily at midnight

        // Act
        var act = async () => await service.ScheduleRecurringAsync(jobId, () => Task.CompletedTask, cronExpression);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NoOpBackgroundJobService_MultipleEnqueues_ShouldAllComplete()
    {
        // Arrange
        var service = new NoOpBackgroundJobService();
        var testService = new TestService();

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(service.EnqueueAsync<TestService>(s => s.DoWorkAsync()));
        }

        var act = async () => await Task.WhenAll(tasks);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NoOpBackgroundJobService_EnqueueWithZeroDelay_ShouldComplete()
    {
        // Arrange
        var service = new NoOpBackgroundJobService();

        // Act
        var act = async () => await service.EnqueueAsync(() => Task.CompletedTask, TimeSpan.Zero);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NoOpBackgroundJobService_ScheduleMultipleRecurring_ShouldComplete()
    {
        // Arrange
        var service = new NoOpBackgroundJobService();

        // Act
        await service.ScheduleRecurringAsync("job1", () => Task.CompletedTask, "0 * * * *");
        await service.ScheduleRecurringAsync("job2", () => Task.CompletedTask, "0 0 * * *");
        await service.ScheduleRecurringAsync("job3", () => Task.CompletedTask, "*/5 * * * *");

        // Assert - No exception means success
        true.Should().BeTrue();
    }

    #endregion

    #region Test Helper Classes

    private class TestService
    {
        public Task DoWorkAsync()
        {
            return Task.CompletedTask;
        }

        public Task DoWorkWithParamsAsync(int value, string message)
        {
            return Task.CompletedTask;
        }
    }

    #endregion
}
