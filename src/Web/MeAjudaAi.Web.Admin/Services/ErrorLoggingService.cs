using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MeAjudaAi.Web.Admin.Services;

/// <summary>
/// Service for logging errors with correlation IDs and stack traces
/// </summary>
public class ErrorLoggingService
{
    private readonly ILogger<ErrorLoggingService> _logger;
    private readonly LiveRegionService _liveRegion;

    public ErrorLoggingService(ILogger<ErrorLoggingService> logger, LiveRegionService liveRegion)
    {
        _logger = logger;
        _liveRegion = liveRegion;
    }

    /// <summary>
    /// Log component render error with full context
    /// </summary>
    public void LogComponentError(Exception exception, string? componentName = null)
    {
        var correlationId = Guid.NewGuid().ToString("N");
        var stackTrace = new StackTrace(exception, true);
        
        _logger.LogError(exception,
            "Component render error in {ComponentName}. CorrelationId: {CorrelationId}. " +
            "Message: {Message}. StackTrace: {StackTrace}",
            componentName ?? "Unknown",
            correlationId,
            exception.Message,
            stackTrace.ToString());

        // TODO: Send to monitoring service (Sentry, Application Insights)
        // await SendToMonitoringService(exception, correlationId);
    }

    /// <summary>
    /// Log unhandled error
    /// </summary>
    public void LogUnhandledError(Exception exception, string context = "Application")
    {
        var correlationId = Guid.NewGuid().ToString("N");
        
        _logger.LogCritical(exception,
            "Unhandled error in {Context}. CorrelationId: {CorrelationId}. " +
            "Type: {ExceptionType}. Message: {Message}",
            context,
            correlationId,
            exception.GetType().Name,
            exception.Message);

        _liveRegion.AnnounceError("Ocorreu um erro inesperado. Por favor, recarregue a página.");
    }

    /// <summary>
    /// Log and announce API error
    /// </summary>
    public void LogApiError(string endpoint, int? statusCode, string errorMessage)
    {
        var correlationId = Guid.NewGuid().ToString("N");
        
        _logger.LogWarning(
            "API error on {Endpoint}. CorrelationId: {CorrelationId}. " +
            "StatusCode: {StatusCode}. Message: {Message}",
            endpoint,
            correlationId,
            statusCode ?? 0,
            errorMessage);

        _liveRegion.AnnounceError($"Erro ao acessar {endpoint}: {errorMessage}");
    }

    /// <summary>
    /// Log validation error
    /// </summary>
    public void LogValidationError(string fieldName, string errorMessage)
    {
        _logger.LogInformation(
            "Validation error on field {FieldName}: {Message}",
            fieldName,
            errorMessage);

        _liveRegion.AnnounceValidationErrors(1);
    }

    /// <summary>
    /// Get user-friendly error message based on exception type
    /// </summary>
    public static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            HttpRequestException => "Erro de conexão. Verifique sua internet e tente novamente.",
            TaskCanceledException => "A requisição demorou muito. Tente novamente.",
            UnauthorizedAccessException => "Você não tem permissão para esta ação.",
            ArgumentNullException => "Dados inválidos foram fornecidos.",
            InvalidOperationException => "Esta operação não pode ser realizada no momento.",
            _ => "Ocorreu um erro inesperado. Nossa equipe foi notificada."
        };
    }

    /// <summary>
    /// Check if error should be retried
    /// </summary>
    public static bool ShouldRetry(Exception exception, int attemptCount = 0)
    {
        const int maxRetries = 3;
        
        if (attemptCount >= maxRetries)
            return false;

        return exception is HttpRequestException or TaskCanceledException;
    }
}
