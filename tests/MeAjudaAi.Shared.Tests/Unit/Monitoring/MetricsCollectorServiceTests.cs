using System.Diagnostics.Metrics;
using FluentAssertions;
using MeAjudaAi.Shared.Monitoring;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Monitoring;

public sealed class MetricsCollectorServiceTests : IDisposable
{
    private readonly MeterListener _meterListener;
    private readonly BusinessMetrics _businessMetrics;
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<MetricsCollectorService>> _loggerMock;
    private readonly List<Measurement<long>> _longMeasurements;
    private readonly FakeTimeProvider _timeProvider;

    public MetricsCollectorServiceTests()
    {
        _longMeasurements = new List<Measurement<long>>();
        _timeProvider = new FakeTimeProvider();

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
            _longMeasurements.Add(new Measurement<long>(measurement, tags));
        });

        _meterListener.Start();

        _businessMetrics = new BusinessMetrics();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<MetricsCollectorService>>();

        _scopeFactoryMock.Setup(s => s.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
    }

    [Fact]
    public async Task StartAsync_ShouldCollectMetricsAndHandleCancellation()
    {
        // Arrange
        var service = new MetricsCollectorService(
            _businessMetrics,
            _scopeFactoryMock.Object,
            _timeProvider,
            _loggerMock.Object,
            TimeSpan.FromMilliseconds(1)); // Fast cycle for testing

        using var cts = new CancellationTokenSource();
        
        // Act
        var startTask = service.StartAsync(cts.Token);
        
        // Advance time to trigger collection
        _timeProvider.Advance(TimeSpan.FromMilliseconds(10));
        
        cts.Cancel();
        
        try
        {
            await service.ExecuteTask!;
        }
        catch (OperationCanceledException) { }

        // Assert
        _longMeasurements.Should().NotBeEmpty(); 
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Metrics collector service started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), 
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceptionOccursInMetricsCollection_ShouldLogAndContinueLoop()
    {
        // Arrange
        _scopeFactoryMock.Setup(s => s.CreateScope()).Throws(new Exception("Scope creation failed"));
        
        var service = new MetricsCollectorService(
            _businessMetrics,
            _scopeFactoryMock.Object,
            _timeProvider,
            _loggerMock.Object,
            TimeSpan.FromMilliseconds(1));

        using var cts = new CancellationTokenSource();
        
        // Act
        var startTask = service.StartAsync(cts.Token);
        
        // Advance time to trigger collection and potential error
        _timeProvider.Advance(TimeSpan.FromMilliseconds(10));
        
        cts.Cancel();
        
        try
        {
            await service.ExecuteTask!;
        }
        catch (OperationCanceledException) { }

        // Assert - Verifica se logou o aviso no CollectMetrics
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to collect some metrics") || v.ToString()!.Contains("Failed to get active users count")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    public void Dispose()
    {
        _meterListener.Dispose();
        _businessMetrics.Dispose();
    }
}
