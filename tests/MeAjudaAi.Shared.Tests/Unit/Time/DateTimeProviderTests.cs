using System.Reflection;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Shared.Tests.Unit.Time;

public class DateTimeProviderTests
{
    private readonly IDateTimeProvider _sut;

    public DateTimeProviderTests()
    {
        // Use reflection to create internal DateTimeProvider instance
        var assembly = typeof(IDateTimeProvider).Assembly;
        var type = assembly.GetType("MeAjudaAi.Shared.Time.DateTimeProvider")!;
        _sut = (IDateTimeProvider)Activator.CreateInstance(type)!;
    }

    [Fact]
    public void CurrentDate_ShouldReturnUtcTime()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var result = _sut.CurrentDate();

        // Assert
        var after = DateTime.UtcNow;
        result.Should().BeOnOrAfter(before);
        result.Should().BeOnOrBefore(after);
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void CurrentDate_CalledMultipleTimes_ShouldReturnIncreasingValues()
    {
        // Act
        var first = _sut.CurrentDate();
        Thread.Sleep(10); // Small delay to ensure time passes
        var second = _sut.CurrentDate();

        // Assert
        second.Should().BeOnOrAfter(first);
    }

    [Fact]
    public void CurrentDate_ShouldNotReturnLocalTime()
    {
        // Act
        var result = _sut.CurrentDate();

        // Assert
        result.Kind.Should().NotBe(DateTimeKind.Local);
    }

    [Fact]
    public void CurrentDate_ShouldBeCloseToCurrentTime()
    {
        // Arrange
        var tolerance = TimeSpan.FromSeconds(1);
        var expectedTime = DateTime.UtcNow;

        // Act
        var result = _sut.CurrentDate();

        // Assert
        var difference = (result - expectedTime).Duration();
        difference.Should().BeLessThan(tolerance);
    }
}
