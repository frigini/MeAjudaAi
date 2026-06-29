using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Utilities.Constants;

/// <summary>
/// Constantes para nomes de módulos para evitar magic strings e garantir consistência.
/// Usado por IPermissionProvider e IKeycloakPermissionResolver para identificação de módulos.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ModuleNames
{
    /// <summary>
    /// Módulo de usuários - gerenciamento de usuários, perfis e autenticação
    /// </summary>
    public const string Users = "Users";

    /// <summary>
    /// Módulo de prestadores - cadastro e gestão de prestadores de serviços
    /// </summary>
    public const string Providers = "Providers";

    /// <summary>
    /// Módulo de documentos - upload, verificação e gestão de documentos
    /// </summary>
    public const string Documents = "Documents";

    /// <summary>
    /// Módulo de catálogo de serviços - categorias e serviços disponíveis
    /// </summary>
    public const string ServiceCatalogs = "ServiceCatalogs";

    /// <summary>
    /// Módulo de busca de prestadores - busca geolocalizada e indexação
    /// </summary>
    public const string SearchProviders = "SearchProviders";

    /// <summary>
    /// Módulo de localização - lookup de CEP, geocoding e validações geográficas
    /// </summary>
    public const string Locations = "Locations";

    /// <summary>
    /// Módulo de agendamentos - booking e execução de serviços
    /// </summary>
    public const string Bookings = "Bookings";

    /// <summary>
    /// Módulo de comunicações - email, SMS, push
    /// </summary>
    public const string Communications = "Communications";

    /// <summary>
    /// Módulo de pagamentos - processamento de pagamentos
    /// </summary>
    public const string Payments = "Payments";

    /// <summary>
    /// Módulo de relatórios - analytics e relatórios do sistema (futuro)
    /// </summary>
    public const string Reports = "Reports";

    /// <summary>
    /// Módulo de avaliações - reviews e ratings de prestadores
    /// </summary>
    public const string Ratings = "Ratings";

    /// <summary>
    /// Todos os nomes de módulos implementados.
    /// </summary>
    public static readonly IReadOnlySet<string> ImplementedModules = new HashSet<string>
    {
        Users,
        Providers,
        Documents,
        ServiceCatalogs,
        SearchProviders,
        Locations,
        Bookings,
        Communications,
        Payments,
        Ratings
    };

    /// <summary>
    /// Todos os nomes de módulos conhecidos (implementados + planejados) para validação.
    /// </summary>
    public static readonly IReadOnlySet<string> AllModules = new HashSet<string>
    {
        Users,
        Providers,
        Documents,
        ServiceCatalogs,
        SearchProviders,
        Locations,
        Bookings,
        Communications,
        Payments,
        Reports,
        Ratings
    };

    /// <summary>
    /// Verifica se um nome de módulo é válido (implementado ou planejado).
    /// </summary>
    public static bool IsValid(string moduleName)
        => AllModules.Contains(moduleName);

    /// <summary>
    /// Verifica se um módulo já está implementado.
    /// </summary>
    public static bool IsImplemented(string moduleName)
        => ImplementedModules.Contains(moduleName);
}
