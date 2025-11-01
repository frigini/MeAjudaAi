using System.Text.RegularExpressions;

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

        // Remove caracteres não numéricos
        cpf = Regex.Replace(cpf, @"[^\d]", "");

        // Verifica se tem 11 dígitos
        if (cpf.Length != 11)
            return false;

        // Verifica se todos os dígitos são iguais
        if (cpf.All(c => c == cpf[0]))
            return false;

        return IsValidDocument(cpf, 11, [10, 9, 8, 7, 6, 5, 4, 3, 2], [11, 10, 9, 8, 7, 6, 5, 4, 3, 2]);
    }

    /// <summary>
    /// Valida se um CNPJ é válido.
    /// </summary>
    public static bool IsValidCnpj(this string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        // Remove caracteres não numéricos
        cnpj = Regex.Replace(cnpj, @"[^\d]", "");

        // Verifica se tem 14 dígitos
        if (cnpj.Length != 14)
            return false;

        // Verifica se todos os dígitos são iguais
        if (cnpj.All(c => c == cnpj[0]))
            return false;

        return IsValidDocument(cnpj, 14, [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2], [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
    }

    /// <summary>
    /// Gera um CPF válido para testes.
    /// </summary>
    public static string GenerateValidCpf() =>
        GenerateValidDocument(9, [10, 9, 8, 7, 6, 5, 4, 3, 2], [11, 10, 9, 8, 7, 6, 5, 4, 3, 2]);

    /// <summary>
    /// Gera um CNPJ válido para testes.
    /// </summary>
    public static string GenerateValidCnpj() =>
        GenerateValidDocument(12, [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2], [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);

    /// <summary>
    /// Valida um documento usando os multiplicadores fornecidos.
    /// </summary>
    private static bool IsValidDocument(string document, int expectedLength, int[] firstMultipliers, int[] secondMultipliers)
    {
        if (document.Length != expectedLength)
            return false;

        // Calcula o primeiro dígito verificador
        var firstSum = 0;
        for (int i = 0; i < firstMultipliers.Length; i++)
        {
            firstSum += int.Parse(document[i].ToString()) * firstMultipliers[i];
        }

        var firstDigit = firstSum % 11;
        firstDigit = firstDigit < 2 ? 0 : 11 - firstDigit;

        if (int.Parse(document[firstMultipliers.Length].ToString()) != firstDigit)
            return false;

        // Calcula o segundo dígito verificador
        var secondSum = 0;
        for (int i = 0; i < secondMultipliers.Length; i++)
        {
            secondSum += int.Parse(document[i].ToString()) * secondMultipliers[i];
        }

        var secondDigit = secondSum % 11;
        secondDigit = secondDigit < 2 ? 0 : 11 - secondDigit;

        return int.Parse(document[secondMultipliers.Length].ToString()) == secondDigit;
    }

    /// <summary>
    /// Gera um documento válido usando os multiplicadores fornecidos.
    /// </summary>
    private static string GenerateValidDocument(int baseLength, int[] firstMultipliers, int[] secondMultipliers)
    {
        var random = RandomNumberGenerator.Create();
        var document = new char[baseLength + 2];

        // Gera números aleatórios para os primeiros dígitos
        for (int i = 0; i < baseLength; i++)
        {
            var bytes = new byte[4];
            random.GetBytes(bytes);
            var randomNumber = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 10;
            document[i] = randomNumber.ToString()[0];
        }

        // Calcula o primeiro dígito verificador
        var firstSum = 0;
        for (int i = 0; i < firstMultipliers.Length; i++)
        {
            firstSum += int.Parse(document[i].ToString()) * firstMultipliers[i];
        }

        var firstDigit = firstSum % 11;
        firstDigit = firstDigit < 2 ? 0 : 11 - firstDigit;
        document[baseLength] = firstDigit.ToString()[0];

        // Calcula o segundo dígito verificador
        var secondSum = 0;
        for (int i = 0; i < secondMultipliers.Length; i++)
        {
            secondSum += int.Parse(document[i].ToString()) * secondMultipliers[i];
        }

        var secondDigit = secondSum % 11;
        secondDigit = secondDigit < 2 ? 0 : 11 - secondDigit;
        document[baseLength + 1] = secondDigit.ToString()[0];

        return new string(document);
    }
}
