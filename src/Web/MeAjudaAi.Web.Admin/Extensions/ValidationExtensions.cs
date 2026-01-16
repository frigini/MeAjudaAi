using System.Text.RegularExpressions;
using FluentValidation;

namespace MeAjudaAi.Web.Admin.Extensions;

/// <summary>
/// Extensões de validação para FluentValidation com regras específicas para o contexto brasileiro.
/// </summary>
public static class ValidationExtensions
{
    #region CPF Validation

    /// <summary>
    /// Valida se o CPF possui formato e dígitos verificadores válidos.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidCpf<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(cpf => IsValidCpf(cpf))
            .WithMessage("CPF inválido. Formato esperado: 000.000.000-00 ou 00000000000");
    }

    /// <summary>
    /// Verifica se um CPF é válido.
    /// </summary>
    public static bool IsValidCpf(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        // Remove formatação
        cpf = cpf.Replace(".", "").Replace("-", "").Trim();

        // CPF deve ter 11 dígitos
        if (cpf.Length != 11 || !cpf.All(char.IsDigit))
            return false;

        // Rejeita CPFs com todos os dígitos iguais
        if (cpf.Distinct().Count() == 1)
            return false;

        // Valida primeiro dígito verificador
        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += (cpf[i] - '0') * (10 - i);
        
        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;
        
        if ((cpf[9] - '0') != digit1)
            return false;

        // Valida segundo dígito verificador
        sum = 0;
        for (var i = 0; i < 10; i++)
            sum += (cpf[i] - '0') * (11 - i);
        
        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;
        
        return (cpf[10] - '0') == digit2;
    }

    #endregion

    #region CNPJ Validation

    /// <summary>
    /// Valida se o CNPJ possui formato e dígitos verificadores válidos.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidCnpj<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(cnpj => IsValidCnpj(cnpj))
            .WithMessage("CNPJ inválido. Formato esperado: 00.000.000/0000-00 ou 00000000000000");
    }

    /// <summary>
    /// Verifica se um CNPJ é válido.
    /// </summary>
    public static bool IsValidCnpj(string? cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        // Remove formatação
        cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "").Trim();

        // CNPJ deve ter 14 dígitos
        if (cnpj.Length != 14 || !cnpj.All(char.IsDigit))
            return false;

        // Rejeita CNPJs com todos os dígitos iguais
        if (cnpj.Distinct().Count() == 1)
            return false;

        // Valida primeiro dígito verificador
        var weights1 = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var sum = 0;
        for (var i = 0; i < 12; i++)
            sum += (cnpj[i] - '0') * weights1[i];

        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;

        if ((cnpj[12] - '0') != digit1)
            return false;

        // Valida segundo dígito verificador
        var weights2 = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        sum = 0;
        for (var i = 0; i < 13; i++)
            sum += (cnpj[i] - '0') * weights2[i];

        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;

        return (cnpj[13] - '0') == digit2;
    }

    #endregion

    #region CPF/CNPJ Combined

    /// <summary>
    /// Valida se o documento é um CPF ou CNPJ válido.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidCpfOrCnpj<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(doc => IsValidCpfOrCnpj(doc))
            .WithMessage("Documento inválido. Informe um CPF (11 dígitos) ou CNPJ (14 dígitos) válido");
    }

    /// <summary>
    /// Verifica se um documento é CPF ou CNPJ válido.
    /// </summary>
    public static bool IsValidCpfOrCnpj(string? document)
    {
        if (string.IsNullOrWhiteSpace(document))
            return false;

        var cleaned = document.Replace(".", "").Replace("-", "").Replace("/", "").Trim();

        return cleaned.Length switch
        {
            11 => IsValidCpf(document),
            14 => IsValidCnpj(document),
            _ => false
        };
    }

    #endregion

    #region Phone Validation

    /// <summary>
    /// Valida se o telefone possui formato brasileiro válido.
    /// Aceita: (00) 0000-0000, (00) 00000-0000, 00000000000
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidBrazilianPhone<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(phone => IsValidBrazilianPhone(phone))
            .WithMessage("Telefone inválido. Formato esperado: (00) 00000-0000 ou (00) 0000-0000");
    }

    /// <summary>
    /// Verifica se um telefone brasileiro é válido.
    /// </summary>
    public static bool IsValidBrazilianPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        // Remove formatação
        var cleaned = Regex.Replace(phone, @"[^\d]", "");

        // Deve ter 10 (fixo) ou 11 (celular) dígitos
        if (cleaned.Length != 10 && cleaned.Length != 11)
            return false;

        // DDD não pode ser 00
        if (cleaned.StartsWith("00"))
            return false;

        // Celular deve começar com 9
        if (cleaned.Length == 11 && cleaned[2] != '9')
            return false;

        return true;
    }

    #endregion

    #region Email Validation

    /// <summary>
    /// Valida se o email possui formato válido (RFC 5322 simplificado).
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidEmail<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .EmailAddress()
            .WithMessage("Email inválido. Informe um endereço de email válido");
    }

    #endregion

    #region ZIP Code (CEP) Validation

    /// <summary>
    /// Valida se o CEP possui formato válido (00000-000 ou 00000000).
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidCep<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(cep => IsValidCep(cep))
            .WithMessage("CEP inválido. Formato esperado: 00000-000 ou 00000000");
    }

    /// <summary>
    /// Verifica se um CEP é válido.
    /// </summary>
    public static bool IsValidCep(string? cep)
    {
        if (string.IsNullOrWhiteSpace(cep))
            return false;

        var cleaned = cep.Replace("-", "").Trim();
        return cleaned.Length == 8 && cleaned.All(char.IsDigit);
    }

    #endregion

    #region XSS Sanitization

    private static readonly Regex[] DangerousPatterns =
    {
        new Regex(@"<script[\s\S]*?>[\s\S]*?</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"javascript\s*:", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"data\s*:", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"on\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"<iframe", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"<embed", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"<object", RegexOptions.IgnoreCase | RegexOptions.Compiled)
    };

    /// <summary>
    /// Remove caracteres potencialmente perigosos para prevenir XSS.
    /// Remove: &lt;, &gt;, &lt;script&gt;, tags HTML, javascript:
    /// </summary>
    public static string SanitizeInput(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove tags HTML
        var sanitized = Regex.Replace(input, @"<[^>]*>", string.Empty);

        // Remove javascript: e data: URIs
        sanitized = Regex.Replace(sanitized, @"javascript\s*:", string.Empty, RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"data\s*:", string.Empty, RegexOptions.IgnoreCase);

        // Remove event handlers (onclick, onerror, etc)
        sanitized = Regex.Replace(sanitized, @"on\w+\s*=", string.Empty, RegexOptions.IgnoreCase);

        return sanitized.Trim();
    }

    /// <summary>
    /// Valida que o texto não contém scripts ou HTML potencialmente perigoso.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> NoXss<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(text => !ContainsDangerousContent(text))
            .WithMessage("O texto contém caracteres ou código não permitido");
    }

    /// <summary>
    /// Verifica se o texto contém conteúdo potencialmente perigoso.
    /// </summary>
    private static bool ContainsDangerousContent(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return DangerousPatterns.Any(regex => regex.IsMatch(text));
    }

    #endregion

    #region File Validation

    /// <summary>
    /// Valida o tipo de arquivo permitido.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidFileType<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        params string[] allowedExtensions)
    {
        return ruleBuilder
            .Must(fileName => IsValidFileType(fileName, allowedExtensions))
            .WithMessage($"Tipo de arquivo não permitido. Tipos aceitos: {string.Join(", ", allowedExtensions)}");
    }

    /// <summary>
    /// Verifica se o tipo de arquivo é válido.
    /// </summary>
    private static bool IsValidFileType(string? fileName, string[] allowedExtensions)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return allowedExtensions.Any(ext => 
            ext.Equals(extension, StringComparison.OrdinalIgnoreCase) || 
            ext.Equals(extension.TrimStart('.'), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Valida o tamanho máximo do arquivo.
    /// </summary>
    public static IRuleBuilderOptions<T, long> MaxFileSize<T>(
        this IRuleBuilder<T, long> ruleBuilder,
        long maxSizeInBytes)
    {
        return ruleBuilder
            .LessThanOrEqualTo(maxSizeInBytes)
            .WithMessage($"O arquivo excede o tamanho máximo permitido de {FormatFileSize(maxSizeInBytes)}");
    }

    /// <summary>
    /// Formata o tamanho do arquivo em formato legível.
    /// </summary>
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    #endregion
}
