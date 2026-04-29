using System.Security.Cryptography;

namespace MeAjudaAi.Shared.Extensions;

/// <summary>
/// Extensões para validação e manipulação de documentos brasileiros
/// </summary>
public static class DocumentExtensions
{
    /// <summary>
    /// Valida se um CPF é válido.
    /// </summary>
    public static bool IsValidCpf(this string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        Span<char> digitsOnly = stackalloc char[11];
        int count = 0;
        foreach (var c in cpf)
        {
            if (char.IsDigit(c))
            {
                if (count == 11) return false;
                digitsOnly[count++] = c;
            }
        }

        if (count != 11)
            return false;

        if (AllCharsEqual(digitsOnly))
            return false;

        ReadOnlySpan<int> firstMultipliers = stackalloc int[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        ReadOnlySpan<int> secondMultipliers = stackalloc int[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        return IsValidDocument(digitsOnly, 11, firstMultipliers, secondMultipliers);
    }

    /// <summary>
    /// Valida se um CNPJ é válido.
    /// </summary>
    public static bool IsValidCnpj(this string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        Span<char> digitsOnly = stackalloc char[14];
        int count = 0;
        foreach (var c in cnpj)
        {
            if (char.IsDigit(c))
            {
                if (count == 14) return false;
                digitsOnly[count++] = c;
            }
        }

        if (count != 14)
            return false;

        if (AllCharsEqual(digitsOnly))
            return false;

        ReadOnlySpan<int> firstMultipliers = stackalloc int[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        ReadOnlySpan<int> secondMultipliers = stackalloc int[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        return IsValidDocument(digitsOnly, 14, firstMultipliers, secondMultipliers);
    }

    private static bool AllCharsEqual(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty) return true;
        char first = span[0];
        foreach (var c in span)
        {
            if (c != first) return false;
        }
        return true;
    }

    /// <summary>
    /// Gera um CPF válido para testes.
    /// </summary>
    public static string GenerateValidCpf() =>
        GenerateValidDocument(9, stackalloc int[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 }, stackalloc int[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 });

    /// <summary>
    /// Gera um CNPJ válido para testes.
    /// </summary>
    public static string GenerateValidCnpj() =>
        GenerateValidDocument(12, stackalloc int[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 }, stackalloc int[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 });

    /// <summary>
    /// Valida um documento usando os multiplicadores fornecidos.
    /// </summary>
    private static bool IsValidDocument(ReadOnlySpan<char> document, int expectedLength, ReadOnlySpan<int> firstMultipliers, ReadOnlySpan<int> secondMultipliers)
    {
        if (document.Length != expectedLength)
            return false;

        // Calcula o primeiro dígito verificador
        var firstSum = 0;
        for (int i = 0; i < firstMultipliers.Length; i++)
        {
            firstSum += (document[i] - '0') * firstMultipliers[i];
        }

        var firstDigit = firstSum % 11;
        firstDigit = firstDigit < 2 ? 0 : 11 - firstDigit;

        if ((document[firstMultipliers.Length] - '0') != firstDigit)
            return false;

        // Calcula o segundo dígito verificador
        var secondSum = 0;
        for (int i = 0; i < secondMultipliers.Length; i++)
        {
            secondSum += (document[i] - '0') * secondMultipliers[i];
        }

        var secondDigit = secondSum % 11;
        secondDigit = secondDigit < 2 ? 0 : 11 - secondDigit;

        return (document[secondMultipliers.Length] - '0') == secondDigit;
    }

    /// <summary>
    /// Gera um documento válido usando os multiplicadores fornecidos.
    /// </summary>
    private static string GenerateValidDocument(int baseLength, ReadOnlySpan<int> firstMultipliers, ReadOnlySpan<int> secondMultipliers)
    {
        using var random = RandomNumberGenerator.Create();
        var document = new char[baseLength + 2];

        // Gera números aleatórios para os primeiros dígitos
        for (int i = 0; i < baseLength; i++)
        {
            var bytes = new byte[4];
            random.GetBytes(bytes);
            var randomNumber = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 10;
            document[i] = (char)('0' + randomNumber);
        }

        // Calcula o primeiro dígito verificador
        var firstSum = 0;
        for (int i = 0; i < firstMultipliers.Length; i++)
        {
            firstSum += (document[i] - '0') * firstMultipliers[i];
        }

        var firstDigit = firstSum % 11;
        firstDigit = firstDigit < 2 ? 0 : 11 - firstDigit;
        document[baseLength] = (char)('0' + firstDigit);

        // Calcula o segundo dígito verificador
        var secondSum = 0;
        for (int i = 0; i < secondMultipliers.Length; i++)
        {
            secondSum += (document[i] - '0') * secondMultipliers[i];
        }

        var secondDigit = secondSum % 11;
        secondDigit = secondDigit < 2 ? 0 : 11 - secondDigit;
        document[baseLength + 1] = (char)('0' + secondDigit);

        return new string(document);
    }
}
