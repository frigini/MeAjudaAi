using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Metrics;

namespace MeAjudaAi.Shared.Tests.Unit.Database.Metrics;

[Trait("Category", "Unit")]
[Trait("Component", "Database")]
public class DatabaseMetricsTests : IDisposable
{
    private readonly TestMeterFactory _meterFactory = new();

    [Fact]
    public void RecordQuery_ForSuccessfulQuery_ShouldNotThrow()
    {
        // Arrange
        var metrics = new DatabaseMetrics(_meterFactory);

        // Act
        var act = () => metrics.RecordQuery("SELECT", TimeSpan.FromMilliseconds(100), isSuccess: true);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordQuery_ForSlowQuery_ShouldNotThrow()
    {
        // Arrange
        var metrics = new DatabaseMetrics(_meterFactory);

        // Act
        var act = () => metrics.RecordQuery("SELECT", TimeSpan.FromSeconds(2), isSuccess: true);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordQuery_ForFailedQuery_ShouldNotThrow()
    {
        // Arrange
        var metrics = new DatabaseMetrics(_meterFactory);

        // Act
        var act = () => metrics.RecordQuery("SELECT", TimeSpan.FromMilliseconds(50), isSuccess: false);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordConnectionError_ShouldNotThrow()
    {
        // Arrange
        var metrics = new DatabaseMetrics(_meterFactory);
        var exception = new InvalidOperationException("Connection refused");

        // Act
        var act = () => metrics.RecordConnectionError("dapper_query_multiple", exception);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordEntityFrameworkQuery_ShouldNotThrow()
    {
        // Arrange
        var metrics = new DatabaseMetrics(_meterFactory);

        // Act
        var act = () => metrics.RecordEntityFrameworkQuery("Users", "query", TimeSpan.FromMilliseconds(100));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordDapperQuery_ShouldNotThrow()
    {
        // Arrange
        var metrics = new DatabaseMetrics(_meterFactory);

        // Act
        var act = () => metrics.RecordDapperQuery("query_multiple", TimeSpan.FromMilliseconds(100));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordQuery_WithZeroDuration_ShouldNotThrow()
    {
        // Arrange
        var metrics = new DatabaseMetrics(_meterFactory);

        // Act
        var act = () => metrics.RecordQuery("SELECT", TimeSpan.Zero);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordQuery_WithMaxTimeSpan_ShouldNotThrow()
    {
        // Arrange
        var metrics = new DatabaseMetrics(_meterFactory);

        // Act
        var act = () => metrics.RecordQuery("SELECT", TimeSpan.MaxValue);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void MultipleMetrics_CanBeRecordedSequentially_ShouldNotThrow()
    {
        // Arrange
        var metrics = new DatabaseMetrics(_meterFactory);

        // Act
        var act = () =>
        {
            metrics.RecordQuery("SELECT", TimeSpan.FromMilliseconds(10), isSuccess: true);
            metrics.RecordQuery("INSERT", TimeSpan.FromMilliseconds(20), isSuccess: true);
            metrics.RecordQuery("UPDATE", TimeSpan.FromMilliseconds(30), isSuccess: false);
            metrics.RecordDapperQuery("query_single", TimeSpan.FromMilliseconds(15));
            metrics.RecordEntityFrameworkQuery("Users", "save", TimeSpan.FromMilliseconds(50));
            metrics.RecordConnectionError("dapper_execute", new Exception("timeout"));
        };

        // Assert
        act.Should().NotThrow();
    }

    public void Dispose()
    {
        _meterFactory.Dispose();
        GC.SuppressFinalize(this);
    }
}
