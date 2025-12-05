using FluentAssertions;
using MeAjudaAi.Shared.Monitoring;
using System.Diagnostics.Metrics;

namespace MeAjudaAi.Shared.Tests.Monitoring;

public class BusinessMetricsTests : IDisposable
{
    private readonly BusinessMetrics _metrics;
    private readonly MeterListener _meterListener;
    private readonly Dictionary<string, long> _counterValues = new();
    private readonly Dictionary<string, double> _histogramValues = new();
    private readonly Dictionary<string, long> _gaugeValues = new();

    public BusinessMetricsTests()
    {
        _metrics = new BusinessMetrics();
        _meterListener = new MeterListener();
        
        _meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == "MeAjudaAi.Business")
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };

        _meterListener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            if (instrument.Name.Contains("current"))
                _gaugeValues[instrument.Name] = measurement;
            else
                _counterValues[instrument.Name] = _counterValues.GetValueOrDefault(instrument.Name, 0) + measurement;
        });

        _meterListener.SetMeasurementEventCallback<double>((instrument, measurement, tags, state) =>
        {
            _histogramValues[instrument.Name] = measurement;
        });

        _meterListener.Start();
    }

    [Fact]
    public void RecordUserRegistration_ShouldIncrementCounter()
    {
        // Act
        _metrics.RecordUserRegistration();
        _metrics.RecordUserRegistration();

        // Assert
        _counterValues.Should().ContainKey("meajudaai.users.registrations.total");
        _counterValues["meajudaai.users.registrations.total"].Should().Be(2);
    }

    [Fact]
    public void RecordUserLogin_ShouldIncrementCounter()
    {
        // Act
        _metrics.RecordUserLogin();
        _metrics.RecordUserLogin();
        _metrics.RecordUserLogin();

        // Assert
        _counterValues.Should().ContainKey("meajudaai.users.logins.total");
        _counterValues["meajudaai.users.logins.total"].Should().Be(3);
    }

    [Fact]
    public void RecordHelpRequestCreated_ShouldIncrementCounter()
    {
        // Act
        _metrics.RecordHelpRequestCreated();

        // Assert
        _counterValues.Should().ContainKey("meajudaai.help_requests.created.total");
        _counterValues["meajudaai.help_requests.created.total"].Should().Be(1);
    }

    [Fact]
    public void RecordHelpRequestCompleted_ShouldIncrementCounter()
    {
        // Act
        _metrics.RecordHelpRequestCompleted();
        _metrics.RecordHelpRequestCompleted();

        // Assert
        _counterValues.Should().ContainKey("meajudaai.help_requests.completed.total");
        _counterValues["meajudaai.help_requests.completed.total"].Should().Be(2);
    }

    [Fact]
    public void RecordHelpRequestDuration_ShouldRecordHistogram()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(30.5);

        // Act
        _metrics.RecordHelpRequestDuration(duration);

        // Assert
        _histogramValues.Should().ContainKey("meajudaai.help_requests.duration.seconds");
        _histogramValues["meajudaai.help_requests.duration.seconds"].Should().BeApproximately(30.5, 0.01);
    }

    [Fact]
    public void RecordApiCall_ShouldIncrementCounter()
    {
        // Act
        _metrics.RecordApiCall();

        // Assert
        _counterValues.Should().ContainKey("meajudaai.api.calls.total");
        _counterValues["meajudaai.api.calls.total"].Should().Be(1);
    }

    [Fact]
    public void RecordDatabaseQuery_ShouldRecordHistogram()
    {
        // Arrange
        var duration = TimeSpan.FromMilliseconds(150);

        // Act
        _metrics.RecordDatabaseQuery(duration);

        // Assert
        _histogramValues.Should().ContainKey("meajudaai.database.query.duration.seconds");
        _histogramValues["meajudaai.database.query.duration.seconds"].Should().BeApproximately(0.15, 0.001);
    }

    [Fact]
    public void SetActiveUsers_ShouldUpdateGauge()
    {
        // Act
        _metrics.SetActiveUsers(42);

        // Assert
        _gaugeValues.Should().ContainKey("meajudaai.users.active.current");
        _gaugeValues["meajudaai.users.active.current"].Should().Be(42);
    }

    [Fact]
    public void SetPendingHelpRequests_ShouldUpdateGauge()
    {
        // Act
        _metrics.SetPendingHelpRequests(15);

        // Assert
        _gaugeValues.Should().ContainKey("meajudaai.help_requests.pending.current");
        _gaugeValues["meajudaai.help_requests.pending.current"].Should().Be(15);
    }

    [Fact]
    public void RecordMultipleMetrics_ShouldTrackIndependently()
    {
        // Act
        _metrics.RecordUserRegistration();
        _metrics.RecordUserLogin();
        _metrics.RecordUserLogin();
        _metrics.RecordHelpRequestCreated();
        _metrics.RecordApiCall();
        _metrics.RecordApiCall();
        _metrics.RecordApiCall();

        // Assert
        _counterValues["meajudaai.users.registrations.total"].Should().Be(1);
        _counterValues["meajudaai.users.logins.total"].Should().Be(2);
        _counterValues["meajudaai.help_requests.created.total"].Should().Be(1);
        _counterValues["meajudaai.api.calls.total"].Should().Be(3);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Act
        var act = () => _metrics.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordHelpRequestDuration_WithZeroDuration_ShouldRecordZero()
    {
        // Act
        _metrics.RecordHelpRequestDuration(TimeSpan.Zero);

        // Assert
        _histogramValues["meajudaai.help_requests.duration.seconds"].Should().Be(0);
    }

    [Fact]
    public void RecordDatabaseQuery_WithLongDuration_ShouldRecordCorrectly()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(5.5);

        // Act
        _metrics.RecordDatabaseQuery(duration);

        // Assert
        _histogramValues["meajudaai.database.query.duration.seconds"].Should().BeApproximately(5.5, 0.01);
    }

    public void Dispose()
    {
        _metrics?.Dispose();
        _meterListener?.Dispose();
    }
}
