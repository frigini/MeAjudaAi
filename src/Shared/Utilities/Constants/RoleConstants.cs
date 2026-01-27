namespace MeAjudaAi.Shared.Utilities.Constants;

/// <summary>
/// Constantes centralizadas para roles do Keycloak.
/// Evita duplicação e garante consistência entre código e configuração do Keycloak.
/// </summary>
public static class RoleConstants
{
    // System/Admin roles
    public const string Admin = "admin";
    public const string SystemAdmin = "meajudaai-system-admin";

    // User roles
    public const string UserAdmin = "meajudaai-user-admin";
    public const string UserOperator = "meajudaai-user-operator";
    public const string User = "meajudaai-user";

    // Provider roles
    public const string ProviderAdmin = "meajudaai-provider-admin";
    public const string Provider = "meajudaai-provider";

    // Order roles
    public const string OrderAdmin = "meajudaai-order-admin";
    public const string OrderOperator = "meajudaai-order-operator";

    // Report roles
    public const string ReportAdmin = "meajudaai-report-admin";
    public const string ReportViewer = "meajudaai-report-viewer";

    // Catalog roles
    public const string CatalogManager = "meajudaai-catalog-manager";

    // Location roles
    public const string LocationManager = "meajudaai-location-manager";
}
