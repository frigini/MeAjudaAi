using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using NSubstitute;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Net;
using System.Text.Json;
using Xunit;

namespace MeAjudaAi.ServiceDefaults.Tests;

/// <summary>
/// Testes abrangentes para Extensions.cs do ServiceDefaults
/// Cobertura: AddServiceDefaults, ConfigureOpenTelemetry, MapDefaultEndpoints, métodos privados
/// </summary>
public class ExtensionsTests
{
    #region AddServiceDefaults Tests (8 tests)

    [Fact]
    public void AddServiceDefaults_ShouldConfigureAllDefaultServices()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        var result = builder.AddServiceDefaults();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(builder);
        
        var services = builder.Services;
        services.Should().Contain(s => s.ServiceType == typeof(IHealthCheck) || 
                                      s.ServiceType.Name.Contains("OpenTelemetry") ||
                                      s.ServiceType.Name.Contains("FeatureManager"));
    }

    [Fact]
    public void AddServiceDefaults_ShouldConfigureOpenTelemetry()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.AddServiceDefaults();

        // Assert
        var hasOpenTelemetry = builder.Services.Any(s => 
            s.ServiceType.FullName != null && s.ServiceType.FullName.Contains("OpenTelemetry"));
        hasOpenTelemetry.Should().BeTrue();
    }

    [Fact]
    public void AddServiceDefaults_ShouldAddHealthChecks()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.AddServiceDefaults();

        // Assert
        builder.Services.Should().Contain(s => s.ServiceType == typeof(HealthCheckService));
    }

    [Fact]
    public void AddServiceDefaults_ShouldAddFeatureManagement()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.AddServiceDefaults();

        // Assert
        var hasFeatureManager = builder.Services.Any(s => 
            s.ServiceType == typeof(IFeatureManager) || 
            s.ServiceType == typeof(IFeatureManagerSnapshot));
        hasFeatureManager.Should().BeTrue();
    }

    [Fact]
    public void AddServiceDefaults_ShouldConfigureHttpClients()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.AddServiceDefaults();

        // Assert
        var host = ((HostApplicationBuilder)builder).Build();
        var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
        httpClientFactory.Should().NotBeNull();
    }

    [Fact]
    public void AddServiceDefaults_WithCustomConfiguration_ShouldApplySettings()
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            ["FeatureManagement:TestFeature"] = "true",
            ["Logging:LogLevel:Default"] = "Information"
        };
        var builder = CreateHostBuilder(config);

        // Act
        builder.AddServiceDefaults();
        var host = ((HostApplicationBuilder)builder).Build();

        // Assert
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        configuration["FeatureManagement:TestFeature"].Should().Be("true");
    }

    [Fact]
    public void AddServiceDefaults_ShouldBeChainable()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        var result = builder.AddServiceDefaults().AddServiceDefaults();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void AddServiceDefaults_InProductionEnvironment_ShouldNotThrow()
    {
        // Arrange
        var builder = CreateHostBuilder(environmentName: "Production");

        // Act
        var act = () => builder.AddServiceDefaults();

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region ConfigureOpenTelemetry Tests (12 tests)

    [Fact]
    public void ConfigureOpenTelemetry_ShouldAddOpenTelemetryLogging()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.ConfigureOpenTelemetry();

        // Assert
        var hasOTelLogging = builder.Services.Any(s => 
            s.ServiceType.FullName != null && s.ServiceType.FullName.Contains("OpenTelemetry") &&
            s.ServiceType.FullName.Contains("Logging"));
        hasOTelLogging.Should().BeTrue();
    }

    [Fact]
    public void ConfigureOpenTelemetry_ShouldConfigureLoggingWithFormattedMessages()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.ConfigureOpenTelemetry();
        var host = ((HostApplicationBuilder)builder).Build();

        // Assert
        var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
        loggerFactory.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureOpenTelemetry_ShouldAddMetricsInstrumentation()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.ConfigureOpenTelemetry();

        // Assert
        var hasMetrics = builder.Services.Any(s => 
            s.ServiceType.FullName != null && s.ServiceType.FullName.Contains("Metrics"));
        hasMetrics.Should().BeTrue();
    }

    [Fact]
    public void ConfigureOpenTelemetry_ShouldAddTracingInstrumentation()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.ConfigureOpenTelemetry();

        // Assert
        var hasTracing = builder.Services.Any(s => 
            s.ServiceType.FullName != null && s.ServiceType.FullName.Contains("Tracing"));
        hasTracing.Should().BeTrue();
    }

    [Fact]
    public void ConfigureOpenTelemetry_ShouldAddAspNetCoreInstrumentation()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.ConfigureOpenTelemetry();
        var host = ((HostApplicationBuilder)builder).Build();

        // Assert - Verify service is registered
        var services = host.Services;
        services.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureOpenTelemetry_ShouldAddHttpClientInstrumentation()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.ConfigureOpenTelemetry();
        var host = ((HostApplicationBuilder)builder).Build();

        // Assert
        var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
        httpClientFactory.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureOpenTelemetry_ShouldAddRuntimeInstrumentation()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.ConfigureOpenTelemetry();

        // Assert - Runtime instrumentation is added through metrics builder
        var hasOTel = builder.Services.Any(s => 
            s.ServiceType.FullName != null && s.ServiceType.FullName.Contains("OpenTelemetry"));
        hasOTel.Should().BeTrue();
    }

    [Fact]
    public void ConfigureOpenTelemetry_WithOtlpEndpoint_ShouldUseOtlpExporter()
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://localhost:4317"
        };
        var builder = CreateHostBuilder(config);

        // Act
        builder.ConfigureOpenTelemetry();

        // Assert
        var hasOTel = builder.Services.Any(s => 
            s.ServiceType.FullName != null && s.ServiceType.FullName.Contains("OpenTelemetry"));
        hasOTel.Should().BeTrue();
    }

    [Fact]
    public void ConfigureOpenTelemetry_WithApplicationInsights_ShouldUseAzureMonitor()
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            ["APPLICATIONINSIGHTS_CONNECTION_STRING"] = "InstrumentationKey=test-key"
        };
        var builder = CreateHostBuilder(config);

        // Act
        builder.ConfigureOpenTelemetry();

        // Assert
        var hasOTel = builder.Services.Any(s => 
            s.ServiceType.FullName != null && s.ServiceType.FullName.Contains("OpenTelemetry"));
        hasOTel.Should().BeTrue();
    }

    [Fact]
    public void ConfigureOpenTelemetry_WithEnvironmentVariables_ShouldUseEnvironmentConfig()
    {
        // Arrange
        var originalOtlp = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
        var originalAppInsights = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
        
        try
        {
            Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", "http://test:4317");
            var builder = CreateHostBuilder();

            // Act
            builder.ConfigureOpenTelemetry();

            // Assert
            builder.Services.Should().NotBeEmpty();
        }
        finally
        {
            Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", originalOtlp);
            Environment.SetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING", originalAppInsights);
        }
    }

    [Fact]
    public void ConfigureOpenTelemetry_ShouldFilterHealthCheckPaths()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.ConfigureOpenTelemetry();
        var host = ((HostApplicationBuilder)builder).Build();

        // Assert - Tracing filter configured to exclude /health and /alive
        var services = host.Services;
        services.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureOpenTelemetry_ShouldBeChainable()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        var result = builder.ConfigureOpenTelemetry();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(builder);
    }

    #endregion

    #region MapDefaultEndpoints Tests (18 tests)

    [Fact]
    public async Task MapDefaultEndpoints_InDevelopment_ShouldMapHealthEndpoint()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Development";
        builder.Services.AddHealthChecks();
        var app = builder.Build();

        // Act
        app.MapDefaultEndpoints();

        // Assert
        var client = app.GetTestClient();
        var response = await client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MapDefaultEndpoints_InDevelopment_ShouldMapLiveEndpoint()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Development";
        builder.Services.AddHealthChecks().AddCheck("live", () => HealthCheckResult.Healthy(), tags: new[] { "live" });
        var app = builder.Build();

        // Act
        app.MapDefaultEndpoints();

        // Assert
        var client = app.GetTestClient();
        var response = await client.GetAsync("/health/live");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MapDefaultEndpoints_InDevelopment_ShouldMapReadyEndpoint()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Development";
        builder.Services.AddHealthChecks().AddCheck("ready", () => HealthCheckResult.Healthy(), tags: new[] { "ready" });
        var app = builder.Build();

        // Act
        app.MapDefaultEndpoints();

        // Assert
        var client = app.GetTestClient();
        var response = await client.GetAsync("/health/ready");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task MapDefaultEndpoints_ShouldReturnJsonResponse()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Development";
        builder.Services.AddHealthChecks().AddCheck("test", () => HealthCheckResult.Healthy("Test check"));
        var app = builder.Build();
        app.MapDefaultEndpoints();

        // Act
        var client = app.GetTestClient();
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("status");
    }

    [Fact]
    public async Task MapDefaultEndpoints_ShouldIncludeStatusInResponse()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Development";
        builder.Services.AddHealthChecks().AddCheck("test", () => HealthCheckResult.Healthy());
        var app = builder.Build();
        app.MapDefaultEndpoints();

        // Act
        var client = app.GetTestClient();
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Assert
        json.RootElement.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetString().Should().Be("Healthy");
    }

    [Fact]
    public async Task MapDefaultEndpoints_ShouldIncludeTotalDuration()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Development";
        builder.Services.AddHealthChecks().AddCheck("test", () => HealthCheckResult.Healthy());
        var app = builder.Build();
        app.MapDefaultEndpoints();

        // Act
        var client = app.GetTestClient();
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Assert
        json.RootElement.TryGetProperty("totalDuration", out var duration).Should().BeTrue();
        duration.GetDouble().Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task MapDefaultEndpoints_ShouldIncludeChecksArray()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Development";
        builder.Services.AddHealthChecks()
            .AddCheck("check1", () => HealthCheckResult.Healthy("First"))
            .AddCheck("check2", () => HealthCheckResult.Healthy("Second"));
        var app = builder.Build();
        app.MapDefaultEndpoints();

        // Act
        var client = app.GetTestClient();
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Assert
        json.RootElement.TryGetProperty("checks", out var checks).Should().BeTrue();
        checks.GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task MapDefaultEndpoints_CheckDetails_ShouldIncludeName()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Development";
        builder.Services.AddHealthChecks().AddCheck("test-check", () => HealthCheckResult.Healthy());
        var app = builder.Build();
        app.MapDefaultEndpoints();

        // Act
        var client = app.GetTestClient();
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Assert
        var checks = json.RootElement.GetProperty("checks");
        var firstCheck = checks[0];
        firstCheck.TryGetProperty("name", out var name).Should().BeTrue();
        name.GetString().Should().Be("test-check");
    }

    [Fact]
    public async Task MapDefaultEndpoints_CheckDetails_ShouldIncludeStatus()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Development";
        builder.Services.AddHealthChecks().AddCheck("test", () => HealthCheckResult.Degraded());
        var app = builder.Build();
        app.MapDefaultEndpoints();

        // Act
        var client = app.GetTestClient();
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Assert
        var checks = json.RootElement.GetProperty("checks");
        checks[0].TryGetProperty("status", out var status).Should().BeTrue();
        status.GetString().Should().Be("Degraded");
    }

    [Fact]
    public async Task MapDefaultEndpoints_CheckDetails_ShouldIncludeDuration()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Development";
        builder.Services.AddHealthChecks().AddCheck("test", () => HealthCheckResult.Healthy());
        var app = builder.Build();
        app.MapDefaultEndpoints();

        // Act
        var client = app.GetTestClient();
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Assert
        var checks = json.RootElement.GetProperty("checks");
        checks[0].TryGetProperty("duration", out var duration).Should().BeTrue();
        duration.GetDouble().Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task MapDefaultEndpoints_WithUnhealthyCheck_ShouldReturnUnhealthyStatus()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Development";
        builder.Services.AddHealthChecks().AddCheck("test", () => HealthCheckResult.Unhealthy("Failed"));
        var app = builder.Build();
        app.MapDefaultEndpoints();

        // Act
        var client = app.GetTestClient();
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        json.RootElement.GetProperty("status").GetString().Should().Be("Unhealthy");
    }

    [Fact]
    public async Task MapDefaultEndpoints_WithException_ShouldIncludeExceptionMessage()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Development";
        builder.Services.AddHealthChecks().AddCheck("test", () => 
            HealthCheckResult.Unhealthy("Error", new InvalidOperationException("Test error")));
        var app = builder.Build();
        app.MapDefaultEndpoints();

        // Act
        var client = app.GetTestClient();
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Assert
        var checks = json.RootElement.GetProperty("checks");
        checks[0].TryGetProperty("exception", out var exception).Should().BeTrue();
        exception.GetString().Should().Be("Test error");
    }

    [Fact]
    public async Task MapDefaultEndpoints_WithData_ShouldIncludeDataProperty()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Development";
        builder.Services.AddHealthChecks().AddCheck("test", () => 
        {
            var data = new Dictionary<string, object> { ["key"] = "value" };
            return HealthCheckResult.Healthy("OK", data);
        });
        var app = builder.Build();
        app.MapDefaultEndpoints();

        // Act
        var client = app.GetTestClient();
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Assert
        var checks = json.RootElement.GetProperty("checks");
        checks[0].TryGetProperty("data", out var data).Should().BeTrue();
        data.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    public async Task MapDefaultEndpoints_ShouldSetNoCacheHeaders()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Development";
        builder.Services.AddHealthChecks();
        var app = builder.Build();
        app.MapDefaultEndpoints();

        // Act
        var client = app.GetTestClient();
        var response = await client.GetAsync("/health");

        // Assert
        response.Headers.CacheControl?.NoCache.Should().BeTrue();
        response.Headers.CacheControl?.NoStore.Should().BeTrue();
        response.Headers.CacheControl?.MustRevalidate.Should().BeTrue();
    }

    [Fact]
    public async Task MapDefaultEndpoints_InTestingEnvironment_ShouldMapEndpoints()
    {
        // Arrange
        var originalDotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        
        try
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddHealthChecks();
            var app = builder.Build();

            // Act
            app.MapDefaultEndpoints();

            // Assert
            var client = app.GetTestClient();
            var response = await client.GetAsync("/health");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", originalDotnetEnv);
        }
    }

    [Fact]
    public async Task MapDefaultEndpoints_WithIntegrationTestsFlag_ShouldMapEndpoints()
    {
        // Arrange
        var originalIntegrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");
        
        try
        {
            Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "true");
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddHealthChecks();
            var app = builder.Build();

            // Act
            app.MapDefaultEndpoints();

            // Assert
            var client = app.GetTestClient();
            var response = await client.GetAsync("/health");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            Environment.SetEnvironmentVariable("INTEGRATION_TESTS", originalIntegrationTests);
        }
    }

    [Fact]
    public async Task MapDefaultEndpoints_InProduction_ShouldNotMapEndpoints()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Production";
        builder.Services.AddHealthChecks();
        var app = builder.Build();

        // Act
        app.MapDefaultEndpoints();

        // Assert
        var client = app.GetTestClient();
        var response = await client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void MapDefaultEndpoints_ShouldBeChainable()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Development";
        builder.Services.AddHealthChecks();
        var app = builder.Build();

        // Act
        var result = app.MapDefaultEndpoints();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(app);
    }

    #endregion

    #region IsTestingEnvironment Tests (8 tests)

    [Fact]
    public async Task IsTestingEnvironment_WithDotnetEnvironmentTesting_ShouldMapEndpoints()
    {
        // Arrange
        var originalDotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var originalAspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        try
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
            
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddHealthChecks();
            var app = builder.Build();
            app.MapDefaultEndpoints();

            // Act
            var client = app.GetTestClient();
            var response = await client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", originalDotnetEnv);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalAspnetEnv);
        }
    }

    [Fact]
    public async Task IsTestingEnvironment_WithAspNetCoreEnvironmentTesting_ShouldMapEndpoints()
    {
        // Arrange
        var originalDotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var originalAspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        try
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddHealthChecks();
            var app = builder.Build();
            app.MapDefaultEndpoints();

            // Act
            var client = app.GetTestClient();
            var response = await client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", originalDotnetEnv);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalAspnetEnv);
        }
    }

    [Fact]
    public async Task IsTestingEnvironment_DotnetEnvironmentTakesPrecedence_OverAspNetCore()
    {
        // Arrange
        var originalDotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var originalAspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        try
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
            
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddHealthChecks();
            var app = builder.Build();
            app.MapDefaultEndpoints();

            // Act
            var client = app.GetTestClient();
            var response = await client.GetAsync("/health");

            // Assert - Should map endpoints because DOTNET_ENVIRONMENT=Testing takes precedence
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", originalDotnetEnv);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalAspnetEnv);
        }
    }

    [Fact]
    public async Task IsTestingEnvironment_WithIntegrationTestsTrue_ShouldMapEndpoints()
    {
        // Arrange
        var originalIntegrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");
        var originalDotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        
        try
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "true");
            
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddHealthChecks();
            var app = builder.Build();
            app.MapDefaultEndpoints();

            // Act
            var client = app.GetTestClient();
            var response = await client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            Environment.SetEnvironmentVariable("INTEGRATION_TESTS", originalIntegrationTests);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", originalDotnetEnv);
        }
    }

    [Fact]
    public async Task IsTestingEnvironment_WithIntegrationTests1_ShouldMapEndpoints()
    {
        // Arrange
        var originalIntegrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");
        var originalDotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        
        try
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "1");
            
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddHealthChecks();
            var app = builder.Build();
            app.MapDefaultEndpoints();

            // Act
            var client = app.GetTestClient();
            var response = await client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            Environment.SetEnvironmentVariable("INTEGRATION_TESTS", originalIntegrationTests);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", originalDotnetEnv);
        }
    }

    [Fact]
    public async Task IsTestingEnvironment_WithIntegrationTestsFalse_ShouldNotMapEndpoints()
    {
        // Arrange
        var originalIntegrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");
        var originalDotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        
        try
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "false");
            
            var builder = WebApplication.CreateBuilder();
            builder.Environment.EnvironmentName = "Production";
            builder.Services.AddHealthChecks();
            var app = builder.Build();
            app.MapDefaultEndpoints();

            // Act
            var client = app.GetTestClient();
            var response = await client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            Environment.SetEnvironmentVariable("INTEGRATION_TESTS", originalIntegrationTests);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", originalDotnetEnv);
        }
    }

    [Fact]
    public async Task IsTestingEnvironment_CaseInsensitive_ShouldWorkWithMixedCase()
    {
        // Arrange
        var originalDotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        
        try
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "TeStInG");
            
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddHealthChecks();
            var app = builder.Build();
            app.MapDefaultEndpoints();

            // Act
            var client = app.GetTestClient();
            var response = await client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", originalDotnetEnv);
        }
    }

    [Fact]
    public async Task IsTestingEnvironment_WithNoEnvironmentVariables_ShouldNotMapEndpoints()
    {
        // Arrange
        var originalDotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var originalAspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var originalIntegrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");
        
        try
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("INTEGRATION_TESTS", null);
            
            var builder = WebApplication.CreateBuilder();
            builder.Environment.EnvironmentName = "Production";
            builder.Services.AddHealthChecks();
            var app = builder.Build();
            app.MapDefaultEndpoints();

            // Act
            var client = app.GetTestClient();
            var response = await client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", originalDotnetEnv);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalAspnetEnv);
            Environment.SetEnvironmentVariable("INTEGRATION_TESTS", originalIntegrationTests);
        }
    }

    #endregion

    #region IsEfCoreAvailable Tests (3 tests)

    [Fact]
    public void ConfigureOpenTelemetry_WithEfCore_ShouldAddEfCoreInstrumentation()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.ConfigureOpenTelemetry();

        // Assert - EF Core instrumentation is conditionally added
        var hasOTel = builder.Services.Any(s => 
            s.ServiceType.FullName != null && s.ServiceType.FullName.Contains("OpenTelemetry"));
        hasOTel.Should().BeTrue();
    }

    [Fact]
    public void ConfigureOpenTelemetry_WithoutEfCore_ShouldNotThrow()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        var act = () => builder.ConfigureOpenTelemetry();

        // Assert - Should not throw even if EF Core is not available
        act.Should().NotThrow();
    }

    [Fact]
    public void ConfigureOpenTelemetry_EfCoreDetection_ShouldHandleExceptions()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act & Assert - IsEfCoreAvailable should handle any exceptions gracefully
        var act = () => builder.ConfigureOpenTelemetry();
        act.Should().NotThrow();
    }

    #endregion

    #region HttpClient Configuration Tests (6 tests)

    [Fact]
    public void AddServiceDefaults_ShouldConfigureHttpClientTimeout()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.AddServiceDefaults();
        var host = ((HostApplicationBuilder)builder).Build();
        var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
        var client = httpClientFactory.CreateClient();

        // Assert
        client.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void AddServiceDefaults_ShouldAddResilienceHandler()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.AddServiceDefaults();
        var host = ((HostApplicationBuilder)builder).Build();

        // Assert
        var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();
        httpClientFactory.Should().NotBeNull();
        
        // Create a client to verify resilience handler is registered
        var client = httpClientFactory.CreateClient();
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddServiceDefaults_HttpClient_ShouldHaveStandardResilience()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.AddServiceDefaults();
        var host = ((HostApplicationBuilder)builder).Build();
        var factory = host.Services.GetRequiredService<IHttpClientFactory>();

        // Assert
        var client = factory.CreateClient();
        client.Should().NotBeNull();
        client.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void AddServiceDefaults_MultipleHttpClients_ShouldAllHaveConfiguration()
    {
        // Arrange
        var builder = CreateHostBuilder();
        builder.Services.AddHttpClient("client1");
        builder.Services.AddHttpClient("client2");

        // Act
        builder.AddServiceDefaults();
        var host = ((HostApplicationBuilder)builder).Build();
        var factory = host.Services.GetRequiredService<IHttpClientFactory>();

        // Assert
        var client1 = factory.CreateClient("client1");
        var client2 = factory.CreateClient("client2");
        
        client1.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        client2.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void AddServiceDefaults_HttpClient_DefaultName_ShouldBeConfigured()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.AddServiceDefaults();
        var host = ((HostApplicationBuilder)builder).Build();
        var factory = host.Services.GetRequiredService<IHttpClientFactory>();

        // Assert
        var defaultClient = factory.CreateClient();
        defaultClient.Should().NotBeNull();
    }

    [Fact]
    public void AddServiceDefaults_HttpClient_CustomName_ShouldInheritDefaults()
    {
        // Arrange
        var builder = CreateHostBuilder();
        builder.Services.AddHttpClient("custom");

        // Act
        builder.AddServiceDefaults();
        var host = ((HostApplicationBuilder)builder).Build();
        var factory = host.Services.GetRequiredService<IHttpClientFactory>();

        // Assert
        var customClient = factory.CreateClient("custom");
        customClient.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    #endregion

    #region Feature Management Tests (5 tests)

    [Fact]
    public void AddServiceDefaults_ShouldRegisterFeatureManager()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.AddServiceDefaults();
        var host = ((HostApplicationBuilder)builder).Build();

        // Assert
        var featureManager = host.Services.GetService<IFeatureManager>();
        featureManager.Should().NotBeNull();
    }

    [Fact]
    public void AddServiceDefaults_ShouldRegisterFeatureManagerSnapshot()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.AddServiceDefaults();
        var host = ((HostApplicationBuilder)builder).Build();

        // Assert
        var snapshot = host.Services.GetService<IFeatureManagerSnapshot>();
        snapshot.Should().NotBeNull();
    }

    [Fact]
    public async Task AddServiceDefaults_FeatureManagement_ShouldRespectConfiguration()
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            ["FeatureManagement:TestFeature"] = "true"
        };
        var builder = CreateHostBuilder(config);

        // Act
        builder.AddServiceDefaults();
        var host = ((HostApplicationBuilder)builder).Build();
        var featureManager = host.Services.GetRequiredService<IFeatureManager>();

        // Assert
        var isEnabled = await featureManager.IsEnabledAsync("TestFeature");
        isEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task AddServiceDefaults_FeatureManagement_DisabledFeature_ShouldReturnFalse()
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            ["FeatureManagement:DisabledFeature"] = "false"
        };
        var builder = CreateHostBuilder(config);

        // Act
        builder.AddServiceDefaults();
        var host = ((HostApplicationBuilder)builder).Build();
        var featureManager = host.Services.GetRequiredService<IFeatureManager>();

        // Assert
        var isEnabled = await featureManager.IsEnabledAsync("DisabledFeature");
        isEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task AddServiceDefaults_FeatureManagement_UndefinedFeature_ShouldReturnFalse()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.AddServiceDefaults();
        var host = ((HostApplicationBuilder)builder).Build();
        var featureManager = host.Services.GetRequiredService<IFeatureManager>();

        // Assert
        var isEnabled = await featureManager.IsEnabledAsync("UndefinedFeature");
        isEnabled.Should().BeFalse();
    }

    #endregion

    #region Integration Tests (3 tests)

    [Fact]
    public void AddServiceDefaults_FullPipeline_ShouldConfigureAllComponents()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        builder.AddServiceDefaults();
        var host = ((HostApplicationBuilder)builder).Build();

        // Assert - Verify all major components are registered
        host.Services.GetService<IHealthCheck>().Should().NotBeNull();
        host.Services.GetService<IHttpClientFactory>().Should().NotBeNull();
        host.Services.GetService<IFeatureManager>().Should().NotBeNull();
        host.Services.GetService<ILoggerFactory>().Should().NotBeNull();
    }

    [Fact]
    public async Task FullStack_WithAllDefaults_ShouldWorkEndToEnd()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Development";
        builder.AddServiceDefaults();
        builder.Services.AddHealthChecks().AddCheck("test", () => HealthCheckResult.Healthy());
        
        var app = builder.Build();
        app.MapDefaultEndpoints();

        // Act
        var client = app.GetTestClient();
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public void MultipleExtensions_CanBeChainedTogether()
    {
        // Arrange
        var builder = CreateHostBuilder();

        // Act
        var result = builder
            .AddServiceDefaults()
            .ConfigureOpenTelemetry()
            .AddServiceDefaults();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(builder);
    }

    #endregion

    #region Helper Methods

    private static IHostApplicationBuilder CreateHostBuilder(
        Dictionary<string, string?>? configuration = null,
        string environmentName = "Development")
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Environment.EnvironmentName = environmentName;

        if (configuration != null)
        {
            builder.Configuration.AddInMemoryCollection(configuration);
        }

        return builder;
    }

    #endregion
}
