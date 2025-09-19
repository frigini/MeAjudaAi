using Azure.Monitor.OpenTelemetry.AspNetCore;
using MeAjudaAi.Shared.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Text.Json;

namespace MeAjudaAi.ServiceDefaults;

public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

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
            http.AddServiceDiscovery();
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
        if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
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
}