namespace MeAjudaAi.Shared.Authorization;

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
    /// Módulo de serviços - catálogo de serviços e categorias (futuro)
    /// </summary>
    public const string Services = "Services";

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
    /// Módulo administrativo - funcionalidades de administração (futuro)
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Todos os nomes de módulos conhecidos para validação
    /// </summary>
    public static readonly IReadOnlySet<string> AllModules = new HashSet<string>
    {
        Users,
        Services,
        Bookings,
        Notifications,
        Payments,
        Reports,
        Admin
    };

    /// <summary>
    /// Verifica se um nome de módulo é válido
    /// </summary>
    /// <param name="moduleName">Nome do módulo para validar</param>
    /// <returns>True se o módulo é conhecido e válido</returns>
    public static bool IsValidModule(string moduleName)
        => !string.IsNullOrWhiteSpace(moduleName) && AllModules.Contains(moduleName);
}
