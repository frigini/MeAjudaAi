namespace MeAjudaAi.Shared.Security;

/// <summary>
/// Papéis do sistema para autorização e controle de acesso
/// </summary>
public static class UserRoles
{
    /// <summary>
    /// Usuário comum com permissões básicas
    /// </summary>
    public const string User = "user";
    
    /// <summary>
    /// Administrador com permissões elevadas
    /// </summary>
    public const string Admin = "admin";
    
    /// <summary>
    /// Super administrador com acesso total ao sistema
    /// </summary>
    public const string SuperAdmin = "super-admin";
    
    /// <summary>
    /// Papel de prestador de serviço para contas empresariais
    /// </summary>
    public const string ServiceProvider = "service-provider";
    
    /// <summary>
    /// Papel de cliente para contas de usuário final
    /// </summary>
    public const string Customer = "customer";
    
    /// <summary>
    /// Papel de moderador para gestão de conteúdo (uso futuro)
    /// </summary>
    public const string Moderator = "moderator";

    /// <summary>
    /// Obtém todos os papéis disponíveis no sistema
    /// </summary>
    public static readonly string[] AllRoles = 
    [
        User,
        Admin,
        SuperAdmin,
        ServiceProvider,
        Customer,
        Moderator
    ];

    /// <summary>
    /// Obtém papéis que possuem privilégios administrativos
    /// </summary>
    public static readonly string[] AdminRoles = 
    [
        Admin,
        SuperAdmin
    ];

    /// <summary>
    /// Obtém papéis disponíveis para criação de usuário comum
    /// </summary>
    public static readonly string[] BasicRoles = 
    [
        User,
        Customer,
        ServiceProvider
    ];

    /// <summary>
    /// Valida se um papel é válido no sistema
    /// </summary>
    /// <param name="role">Papel a ser validado</param>
    /// <returns>True se o papel for válido, false caso contrário</returns>
    public static bool IsValidRole(string role)
    {
        return AllRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Valida se um papel possui privilégios administrativos
    /// </summary>
    /// <param name="role">Papel a ser verificado</param>
    /// <returns>True se o papel for de nível admin, false caso contrário</returns>
    public static bool IsAdminRole(string role)
    {
        return AdminRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}