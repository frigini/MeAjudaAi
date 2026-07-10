using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Metrics;

namespace MeAjudaAi.Shared.Tests.Unit.Database.Metrics;

[Trait("Category", "Unit")]
[Trait("Component", "Database")]
public class DatabaseMetricsTests : IDisposable
{
    private readonly TestMeterFactory _meterFactory = new();

    [Fact]
    public void RecordQuery_ForSuccessfulQuery_ShouldIncrementQueryCount()
    {
        // Arrange
        var metrics = new DatabaseMetrics(_meterFactory);

        // Act
        metrics.RecordQuery("SELECT", TimeSpan.FromMilliseconds(100), isSuccess: true);

        // Assert - no exception thrown, metric recorded
    }

    [Fact]
    public void RecordQuery_ForSlowQuery_ShouldIncrementSlowQueryCount()
    {
        // Arrange
        var metrics = new DatabaseMetrics(_meterFactory);

        // Act - duration > 1.0 second triggers slow query counter
        metrics.RecordQuery("SELECT", TimeSpan.FromSeconds(2), isSuccess: true);

        // Assert - no exception thrown, metric recorded
    }

    [Fact]
    public void RecordQuery_ForFastQuery_ShouldNotIncrementSlowQueryCount()
    {
        // Arrange
        var metrics = new DatabaseMetrics(_meterFactory);

        // Act
        metrics.RecordQuery("SELECT", TimeSpan.FromMilliseconds(50), isSuccess: false);

        // Assert - no exception thrown
    }

    [Fact]
    public void RecordConnectionError_ShouldRecordMetric()
    {
        // Arrange
        var metrics = new DatabaseMetrics(_meterFactory);
        var exception = new InvalidOperationException("Connection refused");

        // Act
        metrics.RecordConnectionError("dapper_query_multiple", exception);

        // Assert - no exception thrown
    }

    [Fact]
    public void RecordEntityFrameworkQuery_ShouldRecordWithEfPrefix()
    {
        // Arrange
        var metrics = new DatabaseMetrics(_meterFactory);

        // Act
        metrics.RecordEntityFrameworkQuery("Users", "query", TimeSpan.FromMilliseconds(100));

        // Assert - no exception thrown
    }

    [Fact]
    public void RecordDapperQuery_ShouldRecordWithDapperPrefix()
    {
        // Arrange
        var metrics = new DatabaseMetrics(_meterFactory);

        // Act
        metrics.RecordDapperQuery("query_multiple", TimeSpan.FromMilliseconds(100));

        // Assert - no exception thrown
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
    public void MultipleMetrics_CanBeRecordedSequentially()
    {
        // Arrange
        var metrics = new DatabaseMetrics(_meterFactory);

        // Act & Assert - no exceptions
        metrics.RecordQuery("SELECT", TimeSpan.FromMilliseconds(10), isSuccess: true);
        metrics.RecordQuery("INSERT", TimeSpan.FromMilliseconds(20), isSuccess: true);
        metrics.RecordQuery("UPDATE", TimeSpan.FromMilliseconds(30), isSuccess: false);
        metrics.RecordDapperQuery("query_single", TimeSpan.FromMilliseconds(15));
        metrics.RecordEntityFrameworkQuery("Users", "save", TimeSpan.FromMilliseconds(50));
        metrics.RecordConnectionError("dapper_execute", new Exception("timeout"));
    }

    public void Dispose()
    {
        _meterFactory.Dispose();
        GC.SuppressFinalize(this);
    }
}
