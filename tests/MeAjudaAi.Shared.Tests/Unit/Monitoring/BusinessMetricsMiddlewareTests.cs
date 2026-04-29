using System.Diagnostics.Metrics;
using System.Linq;
using FluentAssertions;
using MeAjudaAi.Shared.Monitoring;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Monitoring;

public sealed class BusinessMetricsMiddlewareTests : IDisposable
{
    private readonly MeterListener _meterListener;
    private readonly Mock<ILogger<BusinessMetricsMiddleware>> _loggerMock;
    private readonly BusinessMetrics _businessMetrics;
    private readonly List<Measurement<long>> _longMeasurements;
    private readonly List<Measurement<double>> _doubleMeasurements;

    public BusinessMetricsMiddlewareTests()
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
            _longMeasurements.Add(new Measurement<long>(measurement, tags));
        });

        _meterListener.SetMeasurementEventCallback<double>((_, measurement, tags, _) =>
        {
            _doubleMeasurements.Add(new Measurement<double>(measurement, tags));
        });

        _meterListener.Start();

        _loggerMock = new Mock<ILogger<BusinessMetricsMiddleware>>();
        _businessMetrics = new BusinessMetrics();
    }

    [Fact]
    public async Task InvokeAsync_ShouldRecordApiCallAndInvokeNext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test/123";
        context.Request.Method = "GET";
        context.Response.StatusCode = 200;

        var isNextInvoked = false;
        RequestDelegate next = (ctx) =>
        {
            isNextInvoked = true;
            return Task.CompletedTask;
        };

        var middleware = new BusinessMetricsMiddleware(next, _businessMetrics, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        isNextInvoked.Should().BeTrue();
        
        var apiMetrics = _longMeasurements.Where(m => m.Tags.ToArray().Any(t => t.Key == "endpoint" && ((string)t.Value!) == "/api/test/{id}")).ToList();
        apiMetrics.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("/api/users")]
    [InlineData("/api/v1/users")]
    public async Task InvokeAsync_WhenUserRegisters_ShouldRecordUserRegistration(string path)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = "POST";
        context.Response.StatusCode = 201;

        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new BusinessMetricsMiddleware(next, _businessMetrics, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var registrationMetrics = _longMeasurements.Where(m => m.Tags.ToArray().Any(t => t.Key == "source" && ((string)t.Value!) == "api")).ToList();
        registrationMetrics.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/api/v2/auth/login")]
    public async Task InvokeAsync_WhenUserLogsIn_ShouldRecordUserLogin(string path)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = "POST";
        context.Response.StatusCode = 200;

        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new BusinessMetricsMiddleware(next, _businessMetrics, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var loginMetrics = _longMeasurements.Where(m => m.Tags.ToArray().Any(t => t.Key == "method" && ((string)t.Value!) == "password")).ToList();
        loginMetrics.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("/api/help-requests")]
    [InlineData("/api/v1/help-requests")]
    public async Task InvokeAsync_WhenHelpRequestCreated_ShouldRecordHelpRequestCreated(string path)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = "POST";
        context.Response.StatusCode = 201;

        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new BusinessMetricsMiddleware(next, _businessMetrics, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var helpRequestMetrics = _longMeasurements.Where(m => m.Tags.ToArray().Any(t => t.Key == "category" && ((string)t.Value!) == "general")).ToList();
        helpRequestMetrics.Should().HaveCount(1); // One for created
    }

    [Theory]
    [InlineData("/api/v1/help-requests/123/complete")]
    [InlineData("/api/v1/help-requests/543/complete")]
    public async Task InvokeAsync_WhenHelpRequestCompleted_ShouldRecordHelpRequestCompleted(string path)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = "POST";
        context.Response.StatusCode = 204;

        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new BusinessMetricsMiddleware(next, _businessMetrics, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var helpRequestCompletedMetrics = _longMeasurements.Where(m => m.Tags.ToArray().Any(t => t.Key == "endpoint" && ((string)t.Value!).Contains("complete")) == false && m.Tags.ToArray().Any(t => t.Key == "category" && ((string)t.Value!) == "general")).ToList();
        
        // Exclude the API call metric if it was also tracked, focus on the business metric counter.
        // BusinessMetrics internally tracks this via `_helpRequestsCompleted` counter.
        helpRequestCompletedMetrics.Should().NotBeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_WhenPathHasNoVersionNumber_ShouldNOTRecordBusinessMetrics()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/validation/users";
        context.Request.Method = "POST";
        context.Response.StatusCode = 201;

        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new BusinessMetricsMiddleware(next, _businessMetrics, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // Não deve registrar métricas específicas de negócio (user registration, login, help requests, etc)
        // se o caminho não for reconhecido como versionado.
        // Métricas de API (RecordApiCall) ainda são registradas, então verificamos as métricas de negócio específicas.
        var businessMetricsList = _longMeasurements.Where(m => 
        {
            var tagsArray = m.Tags.ToArray();
            return tagsArray.Any(t => t.Key == "category" || t.Key == "type");
        }).ToList();
        businessMetricsList.Should().BeEmpty();
    }

    public void Dispose()
    {
        _meterListener.Dispose();
        _businessMetrics.Dispose();
    }
}
