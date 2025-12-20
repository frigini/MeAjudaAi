using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Shared.Tests.Unit.Time;

public class DateTimeProviderTests
{
    private readonly IDateTimeProvider _sut;

    public DateTimeProviderTests()
    {
        _sut = new DateTimeProvider();
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
        var second = _sut.CurrentDate();

        // Assert
        // O tempo pode ser igual se as chamadas forem rápidas o suficiente, mas nunca deve voltar atrás
        second.Should().BeOnOrAfter(first);
    }

    [Fact]
    public void CurrentDate_ShouldBeCloseToCurrentTime()
    {
        // Arrange
        var tolerance = TimeSpan.FromSeconds(1);

        // Act
        var before = DateTime.UtcNow;
        var result = _sut.CurrentDate();
        var after = DateTime.UtcNow;

        // Assert
        result.Should().BeCloseTo(before, tolerance);
        result.Should().BeCloseTo(after, tolerance);
    }
}
