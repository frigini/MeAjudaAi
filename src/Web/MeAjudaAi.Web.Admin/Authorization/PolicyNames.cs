namespace MeAjudaAi.Web.Admin.Authorization;

/// <summary>
/// Nomes das políticas de autorização disponíveis no sistema.
/// </summary>
public static class PolicyNames
{
    /// <summary>
    /// Política para administradores do sistema.
    /// Requer role "admin".
    /// </summary>
    public const string AdminPolicy = "AdminPolicy";

    /// <summary>
    /// Política para gerenciamento de provedores.
    /// Requer role "provider-manager" ou "admin".
    /// </summary>
    public const string ProviderManagerPolicy = "ProviderManagerPolicy";

    /// <summary>
    /// Política para revisão de documentos.
    /// Requer role "document-reviewer" ou "admin".
    /// </summary>
    public const string DocumentReviewerPolicy = "DocumentReviewerPolicy";

    /// <summary>
    /// Política para gerenciamento de catálogo de serviços.
    /// Requer role "catalog-manager" ou "admin".
    /// </summary>
    public const string CatalogManagerPolicy = "CatalogManagerPolicy";

    /// <summary>
    /// Política para gerenciamento de cidades permitidas.
    /// Requer permissão "locations:manage".
    /// </summary>
    public const string LocationsManagerPolicy = "LocationsManagerPolicy";

    /// <summary>
    /// Política para visualização de dados (acesso de leitura).
    /// Requer qualquer role autenticada.
    /// </summary>
    public const string ViewerPolicy = "ViewerPolicy";
}
