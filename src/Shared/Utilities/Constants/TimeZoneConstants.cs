using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Utilities.Constants;

/// <summary>
/// Constantes de fusos horários utilizados no sistema.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TimeZoneConstants
{
    /// <summary>
    /// Timezone padrão do sistema (Brasília, IANA format).
    /// Usado como fallback quando nenhum timezone é especificado.
    /// </summary>
    public const string DefaultTimeZoneId = "America/Sao_Paulo";
}
