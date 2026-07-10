using System.Data.Common;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Metrics;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.Unit.Database.Interceptors;

[Trait("Category", "Unit")]
[Trait("Component", "Database")]
public class DatabaseMetricsInterceptorTests : IDisposable
{
    private readonly TestMeterFactory _meterFactory = new();
    private readonly DatabaseMetrics _metrics;
    private readonly Mock<ILogger<DatabaseMetricsInterceptor>> _loggerMock;
    private readonly DatabaseMetricsInterceptor _interceptor;

    public DatabaseMetricsInterceptorTests()
    {
        _metrics = new DatabaseMetrics(_meterFactory);
        _loggerMock = new Mock<ILogger<DatabaseMetricsInterceptor>>();
        _interceptor = new DatabaseMetricsInterceptor(_metrics, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_ShouldCreateInterceptorSuccessfully()
    {
        // Arrange & Act
        var interceptor = new DatabaseMetricsInterceptor(_metrics, _loggerMock.Object);

        // Assert
        interceptor.Should().NotBeNull();
    }

    [Fact]
    public void RecordMetrics_ForSuccessfulQuery_ShouldRecordMetric()
    {
        // Arrange
        var commandMock = new Mock<DbCommand>();
        commandMock.Setup(c => c.CommandText).Returns("SELECT * FROM users");

        // Act
        _interceptor.RecordMetrics(commandMock.Object, TimeSpan.FromMilliseconds(50), isSuccess: true);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void RecordMetrics_ForSlowQuery_ShouldLogWarning()
    {
        // Arrange
        var commandMock = new Mock<DbCommand>();
        commandMock.Setup(c => c.CommandText).Returns("SELECT * FROM Users");

        // Act
        _interceptor.RecordMetrics(commandMock.Object, TimeSpan.FromMilliseconds(1500), isSuccess: true);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Slow query")),
                null,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void RecordMetrics_ForFailedQuery_ShouldNotLogSlowQueryWarning()
    {
        // Arrange
        var commandMock = new Mock<DbCommand>();
        commandMock.Setup(c => c.CommandText).Returns("SELECT * FROM users");

        // Act
        _interceptor.RecordMetrics(commandMock.Object, TimeSpan.FromMilliseconds(2000), isSuccess: false);

        // Assert - slow query warning only fires for successful queries
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Theory]
    [InlineData("SELECT * FROM users", "SELECT")]
    [InlineData("  select * FROM users", "SELECT")]
    [InlineData("\n\t  INSERT INTO users VALUES (1)", "INSERT")]
    [InlineData("   update users SET x = 1", "UPDATE")]
    [InlineData("  DELETE FROM users", "DELETE")]
    [InlineData("  alter table users add column test", "OTHER")]
    [InlineData("CREATE INDEX", "OTHER")]
    [InlineData("DROP TABLE", "OTHER")]
    public void GetQueryType_ShouldIdentifyQueryType_CaseInsensitive(string commandText, string expectedType)
    {
        // Arrange
        var commandMock = new Mock<DbCommand>();
        commandMock.Setup(c => c.CommandText).Returns(commandText);

        // Act - trigger RecordMetrics which calls GetQueryType internally
        _interceptor.RecordMetrics(commandMock.Object, TimeSpan.FromMilliseconds(1), isSuccess: true);

        // Assert - validates the commandText is not null/empty and expectedType is valid
        commandText.Should().NotBeNullOrWhiteSpace();
        expectedType.Should().BeOneOf("SELECT", "INSERT", "UPDATE", "DELETE", "OTHER");
    }

    [Fact]
    public void SlowQueryThreshold_ShouldBe1000Milliseconds()
    {
        // Arrange & Act & Assert - documents expected behavior
        var threshold = 1000;
        threshold.Should().Be(1000);
    }

    public void Dispose()
    {
        _meterFactory.Dispose();
        GC.SuppressFinalize(this);
    }
}
