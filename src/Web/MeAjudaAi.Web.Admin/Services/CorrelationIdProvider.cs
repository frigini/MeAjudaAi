using System.Diagnostics;

namespace MeAjudaAi.Web.Admin.Services;

/// <summary>
/// Implementação padrão do provedor de ID de correlação.
/// Usa rastreamento baseado em Activity quando disponível para correlação distribuída.
/// </summary>
public class CorrelationIdProvider : ICorrelationIdProvider
{
    /// <summary>
    /// Obtém ou cria um ID de correlação.
    /// Prefere Activity.Current?.Id para rastreamento distribuído, usa novo GUID como fallback.
    /// </summary>
    /// <returns>String do ID de correlação</returns>
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
