namespace MeAjudaAi.Shared.Constants;

/// <summary>
/// Constantes para nomes de módulos para evitar magic strings e garantir consistência.
/// Usado principalmente pelos IModulePermissionResolver para identificação de módulos.
/// </summary>
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

    // Módulos planejados para implementação futura

    /// <summary>
    /// Módulo de agendamentos - booking e execução de serviços (futuro)
    /// </summary>
    public const string Bookings = "Bookings";

    /// <summary>
    /// Módulo de notificações - sistema de notificações e comunicação (futuro)
    /// </summary>
    public const string Notifications = "Notifications";

    /// <summary>
    /// Módulo de pagamentos - processamento de pagamentos (futuro)
    /// </summary>
    public const string Payments = "Payments";

    /// <summary>
    /// Módulo de relatórios - analytics e relatórios do sistema (futuro)
    /// </summary>
    public const string Reports = "Reports";

    /// <summary>
    /// Módulo de avaliações - reviews e ratings de prestadores (futuro)
    /// </summary>
    public const string Reviews = "Reviews";

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
        Locations
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
        Notifications,
        Payments,
        Reports,
        Reviews
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
