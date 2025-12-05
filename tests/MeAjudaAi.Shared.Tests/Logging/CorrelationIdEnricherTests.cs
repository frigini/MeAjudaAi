using FluentAssertions;
using MeAjudaAi.Shared.Logging;
using Moq;
using Serilog.Core;
using Serilog.Events;

namespace MeAjudaAi.Shared.Tests.Logging;

public class CorrelationIdEnricherTests
{
    private readonly CorrelationIdEnricher _enricher;

    public CorrelationIdEnricherTests()
    {
        _enricher = new CorrelationIdEnricher();
    }

    [Fact]
    public void Enrich_ShouldAddCorrelationIdProperty()
    {
        // Arrange
        var logEvent = CreateLogEvent();
        var propertyFactory = new SimpleLogEventPropertyFactory();

        // Act
        _enricher.Enrich(logEvent, propertyFactory);

        // Assert
        logEvent.Properties.Should().ContainKey("CorrelationId");
        logEvent.Properties["CorrelationId"].Should().NotBeNull();
    }

    [Fact]
    public void Enrich_ShouldGenerateValidGuid()
    {
        // Arrange
        var logEvent = CreateLogEvent();
        var propertyFactory = new SimpleLogEventPropertyFactory();

        // Act
        _enricher.Enrich(logEvent, propertyFactory);

        // Assert
        var property = logEvent.Properties["CorrelationId"];
        var scalarValue = property.Should().BeOfType<ScalarValue>().Subject;
        var value = scalarValue.Value.Should().BeOfType<string>().Subject;
        
        Guid.TryParse(value, out _).Should().BeTrue("correlation ID should be a valid GUID");
    }

    [Fact]
    public void Enrich_MultipleCalls_ShouldGenerateDifferentIds()
    {
        // Arrange
        var logEvent1 = CreateLogEvent();
        var logEvent2 = CreateLogEvent();
        var propertyFactory = new SimpleLogEventPropertyFactory();

        // Act
        _enricher.Enrich(logEvent1, propertyFactory);
        _enricher.Enrich(logEvent2, propertyFactory);

        // Assert
        var id1 = ((ScalarValue)logEvent1.Properties["CorrelationId"]).Value as string;
        var id2 = ((ScalarValue)logEvent2.Properties["CorrelationId"]).Value as string;

        id1.Should().NotBe(id2, "each enrichment should generate a unique ID");
    }

    [Fact]
    public void Enrich_ShouldNotOverwriteExistingProperty()
    {
        // Arrange
        var existingId = "existing-correlation-id";
        var logEvent = new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            MessageTemplate.Empty,
            [new LogEventProperty("CorrelationId", new ScalarValue(existingId))]);
        var propertyFactory = new SimpleLogEventPropertyFactory();

        // Act
        _enricher.Enrich(logEvent, propertyFactory);

        // Assert
        var id = ((ScalarValue)logEvent.Properties["CorrelationId"]).Value as string;
        id.Should().Be(existingId, "existing correlation ID should not be overwritten");
    }

    private static LogEvent CreateLogEvent()
    {
        return new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            MessageTemplate.Empty,
            []);
    }

    private class SimpleLogEventPropertyFactory : ILogEventPropertyFactory
    {
        public LogEventProperty CreateProperty(string name, object? value, bool destructureObjects = false)
        {
            return new LogEventProperty(name, new ScalarValue(value));
        }
    }
}

public class CorrelationIdEnricherExtensionsTests
{
    [Fact]
    public void WithCorrelationIdEnricher_ShouldReturnLoggerConfiguration()
    {
        // Arrange
        var config = new Serilog.LoggerConfiguration();

        // Act
        var result = config.Enrich.WithCorrelationIdEnricher();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Serilog.LoggerConfiguration>();
    }

    [Fact]
    public void WithCorrelationIdEnricher_ShouldBeChainable()
    {
        // Arrange
        var config = new Serilog.LoggerConfiguration();

        // Act
        var result = config
            .Enrich.WithCorrelationIdEnricher()
            .Enrich.FromLogContext();

        // Assert
        result.Should().NotBeNull();
    }
}
