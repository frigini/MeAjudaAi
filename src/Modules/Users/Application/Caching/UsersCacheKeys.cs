namespace MeAjudaAi.Modules.Users.Application.Caching;

/// <summary>
/// Constantes para chaves de cache específicas do módulo Users.
/// Centraliza a nomenclatura de cache keys para evitar duplicações e conflitos.
/// </summary>
public static class UsersCacheKeys
{
    private const string UserPrefix = "user";
    private const string UsersPrefix = "users";

    /// <summary>
    /// Chave para cache de usuário por ID
    /// </summary>
    public static string UserById(Guid userId) => $"{UserPrefix}:id:{userId}";

    /// <summary>
    /// Chave para cache de usuário por email
    /// </summary>
    public static string UserByEmail(string email) => $"{UserPrefix}:email:{email.ToLowerInvariant()}";

    /// <summary>
    /// Chave para cache de lista paginada de usuários
    /// </summary>
    public static string UsersList(int page, int pageSize, string? filter = null)
    {
        var key = $"{UsersPrefix}:list:{page}:{pageSize}";
        return string.IsNullOrEmpty(filter) ? key : $"{key}:filter:{filter}";
    }

    /// <summary>
    /// Chave para cache de contagem total de usuários
    /// </summary>
    public static string UsersCount(string? filter = null)
    {
        var key = $"{UsersPrefix}:count";
        return string.IsNullOrEmpty(filter) ? key : $"{key}:filter:{filter}";
    }

    /// <summary>
    /// Chave para cache de roles de um usuário
    /// </summary>
    public static string UserRoles(Guid userId) => $"{UserPrefix}:roles:{userId}";

    /// <summary>
    /// Chave para cache de configurações relacionadas a usuários
    /// </summary>
    public const string UserSystemConfig = "user-system-config";

    /// <summary>
    /// Chave para cache de estatísticas de usuários
    /// </summary>
    public const string UserStats = "user-stats";
}
