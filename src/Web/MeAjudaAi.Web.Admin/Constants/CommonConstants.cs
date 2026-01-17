namespace MeAjudaAi.Web.Admin.Constants;

/// <summary>
/// Constantes para status de ativação de recursos (categorias, serviços, cidades).
/// </summary>
public static class ActivationStatus
{
    /// <summary>
    /// Status ativo
    /// </summary>
    public const bool Active = true;

    /// <summary>
    /// Status inativo
    /// </summary>
    public const bool Inactive = false;

    /// <summary>
    /// Retorna o nome de exibição localizado para o status de ativação.
    /// </summary>
    /// <param name="isActive">Indica se está ativo</param>
    /// <returns>Nome em português do status</returns>
    public static string ToDisplayName(bool isActive) => isActive ? "Ativo" : "Inativo";

    /// <summary>
    /// Retorna a cor MudBlazor apropriada para o status de ativação.
    /// </summary>
    /// <param name="isActive">Indica se está ativo</param>
    /// <returns>Cor do MudBlazor</returns>
    public static MudBlazor.Color ToColor(bool isActive) => 
        isActive ? MudBlazor.Color.Success : MudBlazor.Color.Default;

    /// <summary>
    /// Retorna o ícone MudBlazor apropriado para o status de ativação.
    /// </summary>
    /// <param name="isActive">Indica se está ativo</param>
    /// <returns>Nome do ícone MudBlazor</returns>
    public static string ToIcon(bool isActive) => 
        isActive ? MudBlazor.Icons.Material.Filled.CheckCircle : MudBlazor.Icons.Material.Filled.Cancel;
}

/// <summary>
/// Constantes para ações comuns do sistema.
/// </summary>
public static class CommonActions
{
    /// <summary>
    /// Ação de criação
    /// </summary>
    public const string Create = "Create";

    /// <summary>
    /// Ação de atualização
    /// </summary>
    public const string Update = "Update";

    /// <summary>
    /// Ação de exclusão
    /// </summary>
    public const string Delete = "Delete";

    /// <summary>
    /// Ação de ativação
    /// </summary>
    public const string Activate = "Activate";

    /// <summary>
    /// Ação de desativação
    /// </summary>
    public const string Deactivate = "Deactivate";

    /// <summary>
    /// Ação de verificação
    /// </summary>
    public const string Verify = "Verify";

    /// <summary>
    /// Retorna o nome de exibição localizado para a ação.
    /// </summary>
    /// <param name="action">Nome da ação</param>
    /// <returns>Nome em português da ação</returns>
    public static string ToDisplayName(string action) => action switch
    {
        Create => "Criar",
        Update => "Atualizar",
        Delete => "Excluir",
        Activate => "Ativar",
        Deactivate => "Desativar",
        Verify => "Verificar",
        _ => action
    };
}

/// <summary>
/// Constantes para severidade de mensagens e alertas.
/// </summary>
public static class MessageSeverity
{
    /// <summary>
    /// Mensagem de sucesso
    /// </summary>
    public const string Success = "Success";

    /// <summary>
    /// Mensagem de informação
    /// </summary>
    public const string Info = "Info";

    /// <summary>
    /// Mensagem de aviso
    /// </summary>
    public const string Warning = "Warning";

    /// <summary>
    /// Mensagem de erro
    /// </summary>
    public const string Error = "Error";

    /// <summary>
    /// Retorna a severidade MudBlazor apropriada.
    /// </summary>
    /// <param name="severity">Nome da severidade</param>
    /// <returns>Enum de severidade do MudBlazor</returns>
    public static MudBlazor.Severity ToMudSeverity(string severity) => severity switch
    {
        Success => MudBlazor.Severity.Success,
        Info => MudBlazor.Severity.Info,
        Warning => MudBlazor.Severity.Warning,
        Error => MudBlazor.Severity.Error,
        _ => MudBlazor.Severity.Normal
    };
}
