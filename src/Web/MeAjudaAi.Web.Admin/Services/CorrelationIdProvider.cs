using System.Diagnostics;

namespace MeAjudaAi.Web.Admin.Services;

/// <summary>
/// Default implementation of correlation ID provider.
/// Uses Activity-based tracing when available for distributed correlation.
/// </summary>
public class CorrelationIdProvider : ICorrelationIdProvider
{
    /// <summary>
    /// Gets or creates a correlation ID.
    /// Prefers Activity.Current?.Id for distributed tracing, falls back to new GUID.
    /// </summary>
    /// <returns>Correlation ID string</returns>
    public string GetOrCreate()
    {
        // Prefer Activity.Current?.Id for distributed tracing
        // This enables correlation across frontend/backend/services
        if (Activity.Current?.Id != null)
            return Activity.Current.Id;
        
        // Fallback to new GUID (format N = 32 hex digits without hyphens)
        return Guid.NewGuid().ToString("N");
    }
}
