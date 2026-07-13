using MeAjudaAi.Shared.Authorization.Metrics;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization.Metrics;

/// <summary>
/// Testes unitários para OperationTimer.
/// </summary>
public class OperationTimerTests
{
    [Fact]
    public void Constructor_WithValidActions_ShouldCallOnStart()
    {
        // Arrange
        var onStartCalled = false;

        // Act
        using var timer = new OperationTimer(
            () => onStartCalled = true,
            _ => { });

        // Assert
        onStartCalled.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithNullOnStart_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new OperationTimer(null!, _ => { });
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("onStart");
    }

    [Fact]
    public void Constructor_WithNullOnComplete_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new OperationTimer(() => { }, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("onComplete");
    }

    [Fact]
    public void Dispose_ShouldCallOnCompleteWithPositiveElapsed()
    {
        // Arrange
        TimeSpan? capturedElapsed = null;

        // Act
        using (var timer = new OperationTimer(() => { }, elapsed => capturedElapsed = elapsed))
        {
            Thread.Sleep(50); // Simula trabalho
        }

        // Assert
        capturedElapsed.Should().NotBeNull();
        capturedElapsed!.Value.TotalMilliseconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Dispose_CalledTwice_ShouldOnlyCallOnCompleteOnce()
    {
        // Arrange
        var completeCount = 0;

        // Act
        var timer = new OperationTimer(() => { }, _ => completeCount++);
        timer.Dispose();
        timer.Dispose();

        // Assert
        completeCount.Should().Be(1);
    }
}
