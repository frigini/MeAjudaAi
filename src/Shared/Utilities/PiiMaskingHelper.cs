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

    /// <summary>
    /// Mascara um endereço de e-mail (ex: jo**@domain.com).
    /// </summary>
    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "[EMPTY]";
        
        var emailSpan = email.AsSpan();
        var atIndex = emailSpan.IndexOf('@');
        
        if (atIndex == -1) return "***@***";
        
        var name = emailSpan[..atIndex];
        var domain = emailSpan[(atIndex + 1)..];
        
        if (name.Length <= 2) return $"*@{domain.ToString()}";
        
        return $"{name[..2].ToString()}**@{domain.ToString()}";
    }

    /// <summary>
    /// Mascara um número de telefone (ex: +55119****1234).
    /// </summary>
    public static string MaskPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return "[EMPTY]";
        
        var length = phoneNumber.Length;
        if (length <= 4) return "****";
        
        if (length <= 8)
        {
            // Para números curtos, mostra apenas os últimos 2 dígitos
            return new string('*', length - 2) + phoneNumber[^2..];
        }
        
        return $"{phoneNumber[..5]}****{phoneNumber[^4..]}";
    }

    /// <summary>
    /// Mascara um CPF (ex: 123.***.***-00).
    /// </summary>
    public static string MaskCpf(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return "[EMPTY]";
        
        Span<char> digits = stackalloc char[11];
        int count = 0;
        
        foreach (var c in cpf)
        {
            if (char.IsDigit(c))
            {
                if (count < 11)
                {
                    digits[count++] = c;
                }
                else
                {
                    // Tem mais de 11 números, CPF inválido
                    return "****";
                }
            }
        }
        
        if (count != 11) return "****";
        
        return $"{digits[..3].ToString()}.***.***-{digits[^2..].ToString()}";
    }

    /// <summary>
    /// Retorna "[REDACTED]" se o dado não for nulo ou vazio, caso contrário retorna "[EMPTY]".
    /// </summary>
    public static string MaskSensitiveData(string? data)
    {
        if (string.IsNullOrWhiteSpace(data)) return "[EMPTY]";
        return "[REDACTED]";
    }
}
