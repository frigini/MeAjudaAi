using System.Diagnostics;

namespace MeAjudaAi.Web.Admin.Services;

/// <summary>
/// Provides correlation IDs for distributed tracing.
/// </summary>
public interface ICorrelationIdProvider
{
    /// <summary>
    /// Gets or creates a correlation ID.
    /// Prefers Activity.Current?.Id for distributed tracing, falls back to new GUID.
    /// </summary>
    /// <returns>Correlation ID string</returns>
    string GetOrCreate();
}
