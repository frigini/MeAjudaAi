using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Utilities.Constants;

/// <summary>
/// Constantes centralizadas para roles do Keycloak.
/// Evita duplicação e garante consistência entre código e configuração do Keycloak.
/// </summary>
[ExcludeFromCodeCoverage]
public static class RoleConstants
{
    // Roles de sistema/admin
    public const string Admin = "admin";
    public const string SystemAdmin = "meajudaai-system-admin";
    public const string SuperAdmin = "meajudaai-super-admin";

    // Roles de usuários
    public const string UserAdmin = "meajudaai-user-admin";
    public const string UserOperator = "meajudaai-user-operator";
    public const string User = "meajudaai-user";

    // Roles de prestadores
    public const string ProviderAdmin = "meajudaai-provider-admin";
    public const string Provider = "meajudaai-provider";

    // Roles de pedidos
    public const string OrderAdmin = "meajudaai-order-admin";
    public const string OrderOperator = "meajudaai-order-operator";

    // Roles de relatórios
    public const string ReportAdmin = "meajudaai-report-admin";
    public const string ReportViewer = "meajudaai-report-viewer";

    // Roles de catálogo
    public const string CatalogManager = "meajudaai-catalog-manager";

    // Roles de localização
    public const string LocationManager = "meajudaai-location-manager";

    // Papel de cliente para contas de usuário final (Customer App)
    public const string Customer = "customer";

    // Roles legadas para compatibilidade (remover após transição completa)
    public const string LegacySystemAdmin = "system-admin";
    public const string LegacySuperAdmin = "super-admin";

    /// <summary>
    /// Lista de roles equivalentes a administrador para verificações de segurança.
    /// </summary>
    public static readonly string[] AdminEquivalentRoles = 
    [
        Admin,
        SystemAdmin,
        SuperAdmin,
        LegacySystemAdmin,
        LegacySuperAdmin
    ];

    /// <summary>
    /// Lista de roles equivalentes a super-administrador.
    /// </summary>
    public static readonly string[] SuperAdminEquivalentRoles = 
    [
        SuperAdmin,
        LegacySuperAdmin
    ];
}
