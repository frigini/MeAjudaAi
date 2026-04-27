using MeAjudaAi.Shared.Monitoring;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Shared.Tests.Unit.Monitoring;

[Trait("Category", "Unit")]
public class BusinessMetricsTests
{
    [Fact]
    public void Constructor_Should_InitializeWithoutErrors()
    {
        // Act
        using var metrics = new BusinessMetrics("TestMeter");

        // Assert
        metrics.Should().NotBeNull();
    }

    [Fact]
    public void RecordUserRegistration_Should_NotThrow()
    {
        // Arrange
        using var metrics = new BusinessMetrics("TestMeter");

        // Act
        var act = () => metrics.RecordUserRegistration("test-source");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordUserLogin_Should_NotThrow()
    {
        // Arrange
        using var metrics = new BusinessMetrics("TestMeter");

        // Act
        var act = () => metrics.RecordUserLogin("user-123", "oauth");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordApiCall_Should_NotThrow()
    {
        // Arrange
        using var metrics = new BusinessMetrics("TestMeter");

        // Act
        var act = () => metrics.RecordApiCall("/api/v1/test", "GET", 200);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordHelpRequest_Should_NotThrow()
    {
        // Arrange
        using var metrics = new BusinessMetrics("TestMeter");

        // Act
        var act1 = () => metrics.RecordHelpRequestCreated("cat", "urgent");
        var act2 = () => metrics.RecordHelpRequestCompleted("cat", TimeSpan.FromMinutes(5));
        var act3 = () => metrics.RecordHelpRequestDuration(TimeSpan.FromMinutes(5), "cat");

        // Assert
        act1.Should().NotThrow();
        act2.Should().NotThrow();
        act3.Should().NotThrow();
    }

    [Fact]
    public void UpdateGauges_Should_NotThrow()
    {
        // Arrange
        using var metrics = new BusinessMetrics("TestMeter");

        // Act
        var act1 = () => metrics.UpdateActiveUsers(10);
        var act2 = () => metrics.UpdatePendingHelpRequests(5);

        // Assert
        act1.Should().NotThrow();
        act2.Should().NotThrow();
    }

    [Fact]
    public void RecordDatabaseQuery_Should_NotThrow()
    {
        // Arrange
        using var metrics = new BusinessMetrics("TestMeter");

        // Act
        var act = () => metrics.RecordDatabaseQuery(TimeSpan.FromMilliseconds(100), "SELECT");

        // Assert
        act.Should().NotThrow();
    }
}
