using System.Text.Json;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using MeAjudaAi.Shared.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MeAjudaAi.ServiceDefaults;

public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.AddFeatureManagement();

        builder.ConfigureHttpClients();

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = httpContext =>
                        {
                            return !httpContext.Request.Path.StartsWithSegments("/health") &&
                                    !httpContext.Request.Path.StartsWithSegments("/alive");
                        };
                    })
                    .AddHttpClientInstrumentation();

                if (IsEfCoreAvailable())
                {
                    tracing.AddEntityFrameworkCoreInstrumentation();
                }
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder ConfigureHttpClients<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });
        });

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        var config = builder.Configuration;

        // OTEL Configuration via Environment Variables
        var otlpEndpoint = config["OTEL_EXPORTER_OTLP_ENDPOINT"] ??
                          Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

        var applicationInsightsConnectionString = config["APPLICATIONINSIGHTS_CONNECTION_STRING"] ??
                                                Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

        // Use OTLP Exporter if endpoint is configured
        var useOtlpExporter = !string.IsNullOrWhiteSpace(otlpEndpoint);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Use Azure Monitor if Application Insights is configured
        if (!string.IsNullOrEmpty(applicationInsightsConnectionString))
        {
            builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
            {
                options.ConnectionString = applicationInsightsConnectionString;
            });
        }

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || IsTestingEnvironment())
        {
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = WriteHealthCheckResponse,
                AllowCachingResponses = false
            });

            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live"),
                ResponseWriter = WriteHealthCheckResponse,
                AllowCachingResponses = false
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("ready"),
                ResponseWriter = WriteHealthCheckResponse,
                AllowCachingResponses = false
            });
        }

        return app;
    }

    private static bool IsEfCoreAvailable()
    {
        try
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.GetName().Name?.StartsWith("Microsoft.EntityFrameworkCore") == true);
        }
        catch
        {
            return false;
        }
    }

    private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        var isDevelopment = context.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment();

        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.TotalMilliseconds,
                exception = entry.Value.Exception?.Message,
                data = entry.Value.Data.Count > 0 ? entry.Value.Data : null
            }).ToArray()
        }, SerializationDefaults.HealthChecks(isDevelopment));

        context.Response.ContentType = "application/json";
        context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";

        await context.Response.WriteAsync(result);
    }

    /// <summary>
    /// Determines if the current environment is Testing using the same precedence as AppHost EnvironmentHelpers
    /// </summary>
    private static bool IsTestingEnvironment()
    {
        // Check DOTNET_ENVIRONMENT first, then fallback to ASPNETCORE_ENVIRONMENT (same precedence as EnvironmentHelpers)
        var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var aspnetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var envName = !string.IsNullOrEmpty(dotnetEnv) ? dotnetEnv : aspnetEnv;

        if (!string.IsNullOrEmpty(envName) &&
            string.Equals(envName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Check INTEGRATION_TESTS environment variable with robust boolean parsing
        var integrationTestsValue = Environment.GetEnvironmentVariable("INTEGRATION_TESTS");
        if (!string.IsNullOrEmpty(integrationTestsValue))
        {
            // Handle both "true"/"false" and "1"/"0" patterns case-insensitively
            if (bool.TryParse(integrationTestsValue, out var boolResult))
            {
                return boolResult;
            }

            // Handle "1" as true (common in CI/CD environments)
            if (string.Equals(integrationTestsValue, "1", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Configura Microsoft.FeatureManagement para controle dinâmico de features.
    /// Suporta Azure App Configuration ou configuração local via appsettings.json.
    /// </summary>
    private static TBuilder AddFeatureManagement<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // Adicionar Feature Management com suporte a filters (TimeWindow, Percentage, etc)
        builder.Services.AddFeatureManagement(builder.Configuration.GetSection("FeatureManagement"));

        return builder;
    }
}
