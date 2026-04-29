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

        if (phoneNumber[0] != '+')
            return false;

        int digitCount = 0;
        var span = phoneNumber.AsSpan(1);
        
        foreach (var c in span)
        {
            if (c is ' ' or '-' or '.') continue;
            if (!char.IsDigit(c)) return false;
            digitCount++;
        }

        return digitCount is >= 8 and <= 15;
    }
}
