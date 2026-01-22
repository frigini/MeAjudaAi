namespace MeAjudaAi.Shared.Utilities;

/// <summary>
/// Papéis do sistema para autorização e controle de acesso
/// </summary>
public static class UserRoles
{
    /// <summary>
    /// Administrador com permissões elevadas - acesso total ao Admin Portal
    /// </summary>
    public const string Admin = "admin";

    /// <summary>
    /// Gerente de provedores - pode criar, editar e deletar provedores
    /// </summary>
    public const string ProviderManager = "provider-manager";

    /// <summary>
    /// Revisor de documentos - pode revisar e aprovar documentos
    /// </summary>
    public const string DocumentReviewer = "document-reviewer";

    /// <summary>
    /// Gerente de catálogo - pode gerenciar serviços e categorias
    /// </summary>
    public const string CatalogManager = "catalog-manager";

    /// <summary>
    /// Operador com leitura/escrita limitada
    /// </summary>
    public const string Operator = "operator";

    /// <summary>
    /// Visualizador - acesso somente leitura
    /// </summary>
    public const string Viewer = "viewer";

    /// <summary>
    /// Papel de cliente para contas de usuário final (Customer App)
    /// </summary>
    public const string Customer = "customer";

    /// <summary>
    /// Obtém todos os papéis disponíveis no sistema
    /// </summary>
    public static readonly string[] AllRoles =
    [
        Admin,
        ProviderManager,
        DocumentReviewer,
        CatalogManager,
        Operator,
        Viewer,
        Customer
    ];

    /// <summary>
    /// Obtém papéis que possuem privilégios administrativos (Admin Portal)
    /// </summary>
    public static readonly string[] AdminRoles =
    [
        Admin,
        ProviderManager,
        DocumentReviewer,
        CatalogManager,
        Operator
    ];

    /// <summary>
    /// Obtém papéis disponíveis para aplicativo do cliente
    /// </summary>
    public static readonly string[] CustomerRoles =
    [
        Customer
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
