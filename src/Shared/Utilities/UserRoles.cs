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

    // ===== PROVIDER TIER ROLES =====
    // Gerenciados automaticamente via webhook Stripe (módulo de pagamentos futuro).
    // Todo prestador começa como provider-standard (plano gratuito).

    /// <summary>
    /// Prestador de serviços no plano gratuito (Standard).
    /// Atribuído automaticamente no auto-registro.
    /// </summary>
    public const string ProviderStandard = "provider-standard";

    /// <summary>
    /// Prestador de serviços no plano Silver (pago via Stripe).
    /// </summary>
    public const string ProviderSilver = "provider-silver";

    /// <summary>
    /// Prestador de serviços no plano Gold (pago via Stripe).
    /// </summary>
    public const string ProviderGold = "provider-gold";

    /// <summary>
    /// Prestador de serviços no plano Platinum (pago via Stripe).
    /// </summary>
    public const string ProviderPlatinum = "provider-platinum";

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
        Customer,
        ProviderStandard,
        ProviderSilver,
        ProviderGold,
        ProviderPlatinum
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
    /// Obtém todos os papéis de prestador (qualquer tier)
    /// </summary>
    public static readonly string[] ProviderRoles =
    [
        ProviderStandard,
        ProviderSilver,
        ProviderGold,
        ProviderPlatinum
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

    /// <summary>
    /// Valida se um papel é de prestador de serviços (qualquer tier)
    /// </summary>
    public static bool IsProviderRole(string role)
    {
        return ProviderRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}
