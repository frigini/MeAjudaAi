namespace MeAjudaAi.Shared.Utilities;

/// <summary>
/// Utilitário para mascarar informações sensíveis (PII) em logs.
/// </summary>
public static class PiiMaskingHelper
{
    /// <summary>
    /// Mascara um ID de usuário para evitar exposição de PII em logs.
    /// </summary>
    /// <param name="userId">ID do usuário a ser mascarado</param>
    /// <returns>ID mascarado no formato "abc***xyz"</returns>
    public static string MaskUserId(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return "[EMPTY]";

        if (userId.Length <= 6)
            return $"{userId[0]}***{userId[^1]}";

        return $"{userId[..3]}***{userId[^3..]}";
    }
}
