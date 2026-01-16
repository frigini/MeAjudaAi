namespace MeAjudaAi.Web.Admin.Constants;

/// <summary>
/// Constantes relacionadas a tipos de prestadores de serviços.
/// Replica o enum EProviderType do backend para uso no frontend.
/// </summary>
public static class ProviderType
{
    /// <summary>
    /// Tipo não definido
    /// </summary>
    public const int None = 0;

    /// <summary>
    /// Prestador individual (Pessoa Física)
    /// </summary>
    public const int Individual = 1;

    /// <summary>
    /// Empresa (Pessoa Jurídica)
    /// </summary>
    public const int Company = 2;

    /// <summary>
    /// Cooperativa
    /// </summary>
    public const int Cooperative = 3;

    /// <summary>
    /// Autônomo/Freelancer
    /// </summary>
    public const int Freelancer = 4;

    /// <summary>
    /// Retorna o nome de exibição localizado para o tipo de prestador.
    /// </summary>
    /// <param name="type">Valor numérico do tipo</param>
    /// <returns>Nome em português do tipo de prestador</returns>
    public static string ToDisplayName(int type) => type switch
    {
        Individual => "Pessoa Física",
        Company => "Pessoa Jurídica",
        Cooperative => "Cooperativa",
        Freelancer => "Autônomo",
        _ => "Não Definido"
    };

    /// <summary>
    /// Retorna todos os tipos válidos de prestador (exceto None).
    /// </summary>
    /// <returns>Lista de tuplas (value, displayName)</returns>
    public static IEnumerable<(int Value, string DisplayName)> GetAll() =>
        new[]
        {
            (Individual, ToDisplayName(Individual)),
            (Company, ToDisplayName(Company)),
            (Cooperative, ToDisplayName(Cooperative)),
            (Freelancer, ToDisplayName(Freelancer))
        };
}

/// <summary>
/// Constantes para status de verificação de prestadores.
/// Replica o enum EVerificationStatus do backend.
/// </summary>
public static class VerificationStatus
{
    /// <summary>
    /// Status não definido
    /// </summary>
    public const int None = 0;

    /// <summary>
    /// Aguardando verificação
    /// </summary>
    public const int Pending = 1;

    /// <summary>
    /// Verificação em andamento
    /// </summary>
    public const int InProgress = 2;

    /// <summary>
    /// Verificado e aprovado
    /// </summary>
    public const int Verified = 3;

    /// <summary>
    /// Rejeitado na verificação
    /// </summary>
    public const int Rejected = 4;

    /// <summary>
    /// Conta suspensa
    /// </summary>
    public const int Suspended = 5;

    /// <summary>
    /// Retorna o nome de exibição localizado para o status de verificação.
    /// </summary>
    /// <param name="status">Valor numérico do status</param>
    /// <returns>Nome em português do status</returns>
    public static string ToDisplayName(int status) => status switch
    {
        Pending => "Pendente",
        InProgress => "Em Análise",
        Verified => "Verificado",
        Rejected => "Rejeitado",
        Suspended => "Suspenso",
        _ => "Não Definido"
    };

    /// <summary>
    /// Retorna a cor MudBlazor apropriada para o status.
    /// </summary>
    /// <param name="status">Valor numérico do status</param>
    /// <returns>Cor do MudBlazor (Success, Warning, Error, etc.)</returns>
    public static MudBlazor.Color ToColor(int status) => status switch
    {
        Verified => MudBlazor.Color.Success,
        Pending => MudBlazor.Color.Warning,
        InProgress => MudBlazor.Color.Info,
        Rejected => MudBlazor.Color.Error,
        Suspended => MudBlazor.Color.Dark,
        _ => MudBlazor.Color.Default
    };
}

/// <summary>
/// Constantes para status do fluxo de registro de prestadores.
/// Replica o enum EProviderStatus do backend.
/// </summary>
public static class ProviderStatus
{
    /// <summary>
    /// Status não definido
    /// </summary>
    public const int None = 0;

    /// <summary>
    /// Aguardando preenchimento das informações básicas
    /// </summary>
    public const int PendingBasicInfo = 1;

    /// <summary>
    /// Aguardando envio e verificação de documentos
    /// </summary>
    public const int PendingDocumentVerification = 2;

    /// <summary>
    /// Prestador ativo e verificado
    /// </summary>
    public const int Active = 3;

    /// <summary>
    /// Prestador suspenso
    /// </summary>
    public const int Suspended = 4;

    /// <summary>
    /// Prestador rejeitado
    /// </summary>
    public const int Rejected = 5;

    /// <summary>
    /// Retorna o nome de exibição localizado para o status do prestador.
    /// </summary>
    /// <param name="status">Valor numérico do status</param>
    /// <returns>Nome em português do status</returns>
    public static string ToDisplayName(int status) => status switch
    {
        PendingBasicInfo => "Informações Básicas Pendentes",
        PendingDocumentVerification => "Documentos Pendentes",
        Active => "Ativo",
        Suspended => "Suspenso",
        Rejected => "Rejeitado",
        _ => "Não Definido"
    };

    /// <summary>
    /// Retorna a cor MudBlazor apropriada para o status.
    /// </summary>
    /// <param name="status">Valor numérico do status</param>
    /// <returns>Cor do MudBlazor</returns>
    public static MudBlazor.Color ToColor(int status) => status switch
    {
        Active => MudBlazor.Color.Success,
        PendingBasicInfo => MudBlazor.Color.Warning,
        PendingDocumentVerification => MudBlazor.Color.Info,
        Suspended => MudBlazor.Color.Dark,
        Rejected => MudBlazor.Color.Error,
        _ => MudBlazor.Color.Default
    };
}
