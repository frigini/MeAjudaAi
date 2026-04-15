using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Shared.Utilities;

/// <summary>
/// Papéis do sistema para autorização e controle de acesso.
/// Centraliza a lógica de agrupamento de papéis usando as constantes canônicas de RoleConstants.
/// </summary>
public static class UserRoles
{
    /// <summary>
    /// Papel legado 'admin' (Keycloak default).
    /// </summary>
    public const string AdminLegacy = RoleConstants.Admin;

    /// <summary>
    /// Administrador do sistema - acesso total às configurações e logs.
    /// </summary>
    public const string SystemAdmin = RoleConstants.SystemAdmin;

    /// <summary>
    /// Super Administrador - acesso irrestrito ao sistema inteiro.
    /// </summary>
    public const string SuperAdmin = RoleConstants.SuperAdmin;

    /// <summary>
    /// Administrador de usuários - pode gerenciar contas e permissões.
    /// </summary>
    public const string UserAdmin = RoleConstants.UserAdmin;

    /// <summary>
    /// Operador de usuários - leitura e atualização limitada de perfis.
    /// </summary>
    public const string UserOperator = RoleConstants.UserOperator;

    /// <summary>
    /// Usuário básico do sistema.
    /// </summary>
    public const string User = RoleConstants.User;

    /// <summary>
    /// Administrador de prestadores - CRUD completo de perfis de serviço.
    /// </summary>
    public const string ProviderAdmin = RoleConstants.ProviderAdmin;

    /// <summary>
    /// Prestador de serviços - acesso ao painel do prestador.
    /// </summary>
    public const string Provider = RoleConstants.Provider;

    /// <summary>
    /// Administrador de pedidos - gerenciar transações e fluxos de serviço.
    /// </summary>
    public const string OrderAdmin = RoleConstants.OrderAdmin;

    /// <summary>
    /// Operador de pedidos - leitura e atualização de status de serviço.
    /// </summary>
    public const string OrderOperator = RoleConstants.OrderOperator;

    /// <summary>
    /// Administrador de relatórios - criar e exportar dados estatísticos.
    /// </summary>
    public const string ReportAdmin = RoleConstants.ReportAdmin;

    /// <summary>
    /// Visualizador de relatórios - acesso somente leitura a dashboards.
    /// </summary>
    public const string ReportViewer = RoleConstants.ReportViewer;

    /// <summary>
    /// Gerente de catálogo - gerenciar serviços e categorias.
    /// </summary>
    public const string CatalogManager = RoleConstants.CatalogManager;

    /// <summary>
    /// Gerente de localidades - gerenciar cidades e estados atendidos.
    /// </summary>
    public const string LocationManager = RoleConstants.LocationManager;

    /// <summary>
    /// Papel de cliente para contas de usuário final (Customer App).
    /// </summary>
    public const string Customer = RoleConstants.Customer;

    // ===== PROVIDER TIER ROLES =====
    
    /// <summary>
    /// Prestador de serviços no plano gratuito (Standard).
    /// </summary>
    public const string ProviderStandard = "meajudaai-provider-standard";

    /// <summary>
    /// Prestador de serviços no plano Silver.
    /// </summary>
    public const string ProviderSilver = "meajudaai-provider-silver";

    /// <summary>
    /// Prestador de serviços no plano Gold.
    /// </summary>
    public const string ProviderGold = "meajudaai-provider-gold";

    /// <summary>
    /// Prestador de serviços no plano Platinum.
    /// </summary>
    public const string ProviderPlatinum = "meajudaai-provider-platinum";

    /// <summary>
    /// Obtém todos os papéis disponíveis no sistema (Catálogo Canônico).
    /// </summary>
    public static readonly string[] AllRoles =
    [
        AdminLegacy,
        SystemAdmin,
        SuperAdmin,
        UserAdmin,
        UserOperator,
        User,
        ProviderAdmin,
        Provider,
        OrderAdmin,
        OrderOperator,
        ReportAdmin,
        ReportViewer,
        CatalogManager,
        LocationManager,
        Customer,
        ProviderStandard,
        ProviderSilver,
        ProviderGold,
        ProviderPlatinum,
        RoleConstants.LegacySystemAdmin,
        RoleConstants.LegacySuperAdmin
    ];

    /// <summary>
    /// Obtém papéis que possuem privilégios administrativos (Acesso ao Admin Portal).
    /// </summary>
    public static readonly string[] AdminRoles =
    [
        AdminLegacy,
        SystemAdmin,
        SuperAdmin,
        UserAdmin,
        ProviderAdmin,
        OrderAdmin,
        ReportAdmin,
        CatalogManager,
        LocationManager,
        RoleConstants.LegacySystemAdmin,
        RoleConstants.LegacySuperAdmin
    ];

    /// <summary>
    /// Obtém todos os papéis de prestador (qualquer tier).
    /// </summary>
    public static readonly string[] ProviderRoles =
    [
        Provider,
        ProviderStandard,
        ProviderSilver,
        ProviderGold,
        ProviderPlatinum
    ];

    /// <summary>
    /// Valida se um papel é válido no sistema.
    /// </summary>
    public static bool IsValidRole(string role)
    {
        return AllRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Valida se um papel possui privilégios administrativos.
    /// </summary>
    public static bool IsAdminRole(string role)
    {
        return AdminRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Valida se um papel é de prestador de serviços.
    /// </summary>
    public static bool IsProviderRole(string role)
    {
        return ProviderRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}
