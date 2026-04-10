using System.Diagnostics.Metrics;
using FluentAssertions;
using MeAjudaAi.Shared.Monitoring;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Monitoring;

public sealed class BusinessMetricsTests : IDisposable
{
    private readonly MeterListener _meterListener;
    private readonly BusinessMetrics _sut;
    private readonly List<Measurement<long>> _longMeasurements;
    private readonly List<Measurement<double>> _doubleMeasurements;
    private readonly object _lock = new();

    public BusinessMetricsTests()
    {
        _longMeasurements = new List<Measurement<long>>();
        _doubleMeasurements = new List<Measurement<double>>();

        _meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == "MeAjudaAi.Business")
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            }
        };

        _meterListener.SetMeasurementEventCallback<long>((_, measurement, tags, _) =>
        {
            lock (_lock)
            {
                _longMeasurements.Add(new Measurement<long>(measurement, tags));
            }
        });

        _meterListener.SetMeasurementEventCallback<double>((_, measurement, tags, _) =>
        {
            lock (_lock)
            {
                _doubleMeasurements.Add(new Measurement<double>(measurement, tags));
            }
        });

        _meterListener.Start();
        
        _sut = new BusinessMetrics();
    }

    [Fact]
    public void RecordUserRegistration_ShouldIncrementCounterWithSourceTag()
    {
        // Act
        _sut.RecordUserRegistration("mobile");

        // Assert
        Measurement<long> metric;
        lock (_lock) { metric = _longMeasurements.Should().ContainSingle().Subject; }
        metric.Value.Should().Be(1);
        metric.Tags.ToArray().Should().ContainEquivalentOf(new KeyValuePair<string, object?>("source", "mobile"));
    }

    [Fact]
    public void RecordUserLogin_ShouldIncrementCounterWithUserAndMethodTags()
    {
        // Act
        _sut.RecordUserLogin("user-123", "oauth");

        // Assert
        Measurement<long> metric;
        lock (_lock) { metric = _longMeasurements.Should().ContainSingle().Subject; }
        metric.Value.Should().Be(1);
        
        var tags = metric.Tags.ToArray();
        tags.Should().ContainEquivalentOf(new KeyValuePair<string, object?>("user_id", "user-123"));
        tags.Should().ContainEquivalentOf(new KeyValuePair<string, object?>("method", "oauth"));
    }

    [Fact]
    public void UpdateActiveUsers_ShouldRecordGaugeValue()
    {
        // Act
        _sut.UpdateActiveUsers(42);

        // Assert
        Measurement<long> metric;
        lock (_lock) { metric = _longMeasurements.Should().ContainSingle().Subject; }
        metric.Value.Should().Be(42);
    }

    [Fact]
    public void RecordHelpRequestCreated_ShouldIncrementCounterWithCategoryAndUrgency()
    {
        // Act
        _sut.RecordHelpRequestCreated("medical", "high");

        // Assert
        Measurement<long> metric;
        lock (_lock) { metric = _longMeasurements.Should().ContainSingle().Subject; }
        metric.Value.Should().Be(1);
        
        var tags = metric.Tags.ToArray();
        tags.Should().ContainEquivalentOf(new KeyValuePair<string, object?>("category", "medical"));
        tags.Should().ContainEquivalentOf(new KeyValuePair<string, object?>("urgency", "high"));
    }

    [Fact]
    public void RecordHelpRequestCompleted_ShouldIncrementCounterWithCategory()
    {
        // Act
        _sut.RecordHelpRequestCompleted("medical", TimeSpan.FromMinutes(30));

        // Assert
        Measurement<long> metric;
        lock (_lock) { metric = _longMeasurements.Should().ContainSingle().Subject; }
        metric.Value.Should().Be(1);
        metric.Tags.ToArray().Should().ContainEquivalentOf(new KeyValuePair<string, object?>("category", "medical"));
    }

    [Fact]
    public void RecordHelpRequestDuration_ShouldRecordHistogramValueInSeconds()
    {
        // Act
        _sut.RecordHelpRequestDuration(TimeSpan.FromSeconds(120), "plumbing");

        // Assert
        Measurement<double> metric;
        lock (_lock) { metric = _doubleMeasurements.Should().ContainSingle().Subject; }
        metric.Value.Should().Be(120);
        metric.Tags.ToArray().Should().ContainEquivalentOf(new KeyValuePair<string, object?>("category", "plumbing"));
    }

    [Fact]
    public void UpdatePendingHelpRequests_ShouldRecordGaugeValue()
    {
        // Act
        _sut.UpdatePendingHelpRequests(15);

        // Assert
        Measurement<long> metric;
        lock (_lock) { metric = _longMeasurements.Should().ContainSingle().Subject; }
        metric.Value.Should().Be(15);
    }

    [Fact]
    public void RecordApiCall_ShouldIncrementCounterWithEndpointMethodAndStatus()
    {
        // Act
        _sut.RecordApiCall("/api/users", "GET", 200);

        // Assert
        Measurement<long> metric;
        lock (_lock) { metric = _longMeasurements.Should().ContainSingle().Subject; }
        metric.Value.Should().Be(1);
        
        var tags = metric.Tags.ToArray();
        tags.Should().ContainEquivalentOf(new KeyValuePair<string, object?>("endpoint", "/api/users"));
        tags.Should().ContainEquivalentOf(new KeyValuePair<string, object?>("method", "GET"));
        tags.Should().ContainEquivalentOf(new KeyValuePair<string, object?>("status_code", 200));
    }

    [Fact]
    public void RecordDatabaseQuery_ShouldRecordHistogramValueInSeconds()
    {
        // Act
        _sut.RecordDatabaseQuery(TimeSpan.FromMilliseconds(500), "SELECT");

        // Assert
        Measurement<double> metric;
        lock (_lock) { metric = _doubleMeasurements.Should().ContainSingle().Subject; }
        metric.Value.Should().Be(0.5);
        metric.Tags.ToArray().Should().ContainEquivalentOf(new KeyValuePair<string, object?>("operation", "SELECT"));
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _meterListener.Dispose();
            _sut.Dispose();
        }
    }
}
