namespace MeAjudaAi.Shared.Utilities;

/// <summary>
/// Utilitário para validação de números de telefone
/// </summary>
public static class PhoneNumberValidator
{
    /// <summary>
    /// Valida se um número de telefone está no formato internacional
    /// </summary>
    /// <param name="phoneNumber">Número de telefone a validar</param>
    /// <returns>True se válido, False caso contrário</returns>
    public static bool IsValidInternationalFormat(string? phoneNumber)
    {
        // Validação básica para formato internacional: +[código país][número]
        // Deve começar com + e conter 8-15 dígitos
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        if (!phoneNumber.StartsWith('+'))
            return false;

        var digitsOnly = phoneNumber[1..].Replace(" ", "").Replace("-", "");
        return digitsOnly.Length >= 8 && digitsOnly.Length <= 15 && digitsOnly.All(char.IsDigit);
    }
}
