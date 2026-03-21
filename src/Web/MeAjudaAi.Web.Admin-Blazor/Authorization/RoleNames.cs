namespace MeAjudaAi.Web.Admin.Authorization;

/// <summary>
/// Nomes das roles usadas no sistema (sincronizado com Keycloak).
/// </summary>
public static class RoleNames
{
    /// <summary>
    /// Administrador do sistema - acesso total.
    /// </summary>
    public const string Admin = "admin";

    /// <summary>
    /// Gerente de provedores - pode criar, editar e deletar provedores.
    /// </summary>
    public const string ProviderManager = "provider-manager";

    /// <summary>
    /// Revisor de documentos - pode revisar e aprovar documentos.
    /// </summary>
    public const string DocumentReviewer = "document-reviewer";

    /// <summary>
    /// Gerente de catálogo - pode gerenciar serviços e categorias.
    /// </summary>
    public const string CatalogManager = "catalog-manager";

    /// <summary>
    /// Gerente de localidades - pode gerenciar cidades permitidas.
    /// </summary>
    public const string LocationsManager = "locations-manager";

    /// <summary>
    /// Visualizador - acesso somente leitura.
    /// </summary>
    public const string Viewer = "viewer";
}
