using MeAjudaAi.Shared.Authorization;

namespace MeAjudaAi.Shared.Authorization.Metrics;

/// <summary>
/// Interface para o serviço de métricas e monitoramento do sistema de permissões.
/// Permite mock em testes unitários.
/// </summary>
public interface IPermissionMetricsService : IDisposable
{
    /// <summary>
    /// Mede o tempo de resolução de permissões para um usuário.
    /// </summary>
    IDisposable MeasurePermissionResolution(string userId, string? module = null);

    /// <summary>
    /// Mede e registra uma verificação de permissão.
    /// </summary>
    IDisposable MeasurePermissionCheck(string userId, EPermission permission, bool granted);

    /// <summary>
    /// Mede verificação de múltiplas permissões.
    /// </summary>
    IDisposable MeasureMultiplePermissionCheck(string userId, IEnumerable<EPermission> permissions, bool requireAll);

    /// <summary>
    /// Mede resolução de permissões por módulo.
    /// </summary>
    IDisposable MeasureModulePermissionResolution(string userId, string moduleName);

    /// <summary>
    /// Mede operações de cache.
    /// </summary>
    IDisposable MeasureCacheOperation(string operation, bool hit);

    /// <summary>
    /// Registra uma falha de autorização.
    /// </summary>
    void RecordAuthorizationFailure(string userId, EPermission permission, string reason);

    /// <summary>
    /// Registra invalidação de cache.
    /// </summary>
    void RecordCacheInvalidation(string userId, string reason);

    /// <summary>
    /// Registra estatísticas de performance.
    /// </summary>
    void RecordPerformanceStats(string component, double value, string unit = "count");

    /// <summary>
    /// Obtém estatísticas do sistema.
    /// </summary>
    PermissionSystemStats GetSystemStats();
}
