using System.Diagnostics;

namespace MeAjudaAi.Web.Admin.Services;

/// <summary>
/// Fornece IDs de correlação para rastreamento distribuído.
/// </summary>
public interface ICorrelationIdProvider
{
    /// <summary>
    /// Obtém ou cria um ID de correlação.
    /// Prefere Activity.Current?.Id para rastreamento distribuído, usa novo GUID como fallback.
    /// </summary>
    /// <returns>String do ID de correlação</returns>
    string GetOrCreate();
}
