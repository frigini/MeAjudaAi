namespace MeAjudaAi.Contracts.Utilities.Constants;

/// <summary>
/// Constantes compartilhadas para o módulo de Communications.
/// </summary>
public static class CommunicationConstants
{
    /// <summary>
    /// E-mail de fallback do administrador quando a configuração não está definida.
    /// </summary>
    public const string DefaultAdminEmail = "suporte@meajudaai.com.br";

    /// <summary>
    /// Chave de configuração para o e-mail do administrador.
    /// </summary>
    public const string AdminEmailConfigKey = "Communications:AdminEmail";

    /// <summary>
    /// Chave de configuração para a URL base do cliente.
    /// </summary>
    public const string ClientBaseUrlConfigKey = "ClientBaseUrl";

    /// <summary>
    /// Separador utilizado na construção de correlationIds.
    /// </summary>
    public const string CorrelationSeparator = ":";
}
