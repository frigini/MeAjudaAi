namespace MeAjudaAi.Shared.Utilities.Constants;

/// <summary>
/// Constantes centralizadas para roles do Keycloak.
/// Evita duplicação e garante consistência entre código e configuração do Keycloak.
/// </summary>
public static class RoleConstants
{
    // Roles de sistema/admin
    public const string Admin = "admin";
    public const string SystemAdmin = "meajudaai-system-admin";

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
}
