namespace MeAjudaAi.Shared.Caching;

/// <summary>
/// Constantes para tags de cache utilizadas no sistema.
/// Permite invalidação em grupo de entradas relacionadas.
/// </summary>
public static class CacheTags
{
    // Tags para o módulo Users
    public const string Users = "users";
    public const string UserById = "user-by-id";
    public const string UserByEmail = "user-by-email";
    public const string UsersList = "users-list";
    public const string UserRoles = "user-roles";
    
    // Tags gerais do sistema
    public const string Configuration = "configuration";
    public const string Metadata = "metadata";
    
    /// <summary>
    /// Gera tag específica para um usuário
    /// </summary>
    public static string UserTag(Guid userId) => $"user:{userId}";
    
    /// <summary>
    /// Gera tag específica para email de usuário
    /// </summary>
    public static string UserEmailTag(string email) => $"user-email:{email.ToLowerInvariant()}";
    
    /// <summary>
    /// Gera tag para paginação de usuários
    /// </summary>
    public static string UsersPageTag(int page, int pageSize) => $"users-page:{page}:{pageSize}";
    
    /// <summary>
    /// Combina múltiplas tags
    /// </summary>
    public static string[] CombineTags(params string[] tags) => tags;
    
    /// <summary>
    /// Tags relacionadas a um usuário específico
    /// </summary>
    public static string[] GetUserRelatedTags(Guid userId, string? email = null)
    {
        var tags = new List<string>
        {
            Users,
            UserById,
            UserTag(userId),
            UsersList // Invalida listas que podem incluir este usuário
        };
        
        if (!string.IsNullOrEmpty(email))
        {
            tags.Add(UserByEmail);
            tags.Add(UserEmailTag(email));
        }
        
        return tags.ToArray();
    }
}