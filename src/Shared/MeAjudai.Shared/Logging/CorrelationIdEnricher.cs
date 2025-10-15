using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace MeAjudaAi.Shared.Logging;

/// <summary>
/// Enricher do Serilog para adicionar Correlation ID aos logs
/// </summary>
public class CorrelationIdEnricher : ILogEventEnricher
{
    private const string CorrelationIdPropertyName = "CorrelationId";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // Tentar obter correlation ID do contexto atual
        var correlationId = GetCorrelationId();

        if (!string.IsNullOrEmpty(correlationId))
        {
            var property = propertyFactory.CreateProperty(CorrelationIdPropertyName, correlationId);
            logEvent.AddPropertyIfAbsent(property);
        }
    }

    private static string? GetCorrelationId()
    {
        // Tentar obter do HttpContext se disponível
        var httpContextAccessor = GetHttpContextAccessor();
        if (httpContextAccessor?.HttpContext != null)
        {
            var context = httpContextAccessor.HttpContext;

            // Verificar se já existe no response headers
            if (context.Response.Headers.TryGetValue("X-Correlation-ID", out var existingId))
            {
                return existingId.FirstOrDefault();
            }

            // Verificar se veio no request
            if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var requestId))
            {
                return requestId.FirstOrDefault();
            }
        }

        // Gerar novo se não encontrar
        return UuidGenerator.NewIdString();
    }

    private static Microsoft.AspNetCore.Http.IHttpContextAccessor? GetHttpContextAccessor()
    {
        // Tentar obter do ServiceProvider se disponível
        try
        {
            var serviceProvider = GetCurrentServiceProvider();
            return serviceProvider?.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        }
        catch
        {
            return null;
        }
    }

    private static IServiceProvider? GetCurrentServiceProvider()
    {
        // Em um contexto real, isso seria injetado ou obtido do contexto da aplicação
        // Por simplicidade, retornando null por enquanto
        return null;
    }
}

/// <summary>
/// Extension methods para registrar o enricher
/// </summary>
public static class CorrelationIdEnricherExtensions
{
    /// <summary>
    /// Adiciona o enricher de correlation ID
    /// </summary>
    public static LoggerConfiguration WithCorrelationIdEnricher(this LoggerEnrichmentConfiguration enrichConfiguration)
    {
        return enrichConfiguration.With<CorrelationIdEnricher>();
    }
}