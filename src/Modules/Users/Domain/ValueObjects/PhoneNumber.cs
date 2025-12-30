using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Número de telefone.
/// </summary>
public class PhoneNumber : ValueObject
{
    public string Value { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = "BR";

    // Construtor privado para EF Core
    private PhoneNumber()
    {
    }

    public PhoneNumber(string value, string countryCode = "BR")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Telefone não pode ser vazio");
        
        var cleanValue = value.Trim();
        var normalizedCountryCode = countryCode.Trim().ToUpperInvariant();
        
        // Se começa com '+', é formato internacional - extrair código do país
        if (cleanValue.StartsWith('+'))
        {
            var allDigits = new string(cleanValue[1..].Where(char.IsDigit).ToArray());
            
            // Detectar números brasileiros em formato internacional (+55...)
            if (allDigits.StartsWith("55"))
            {
                // Extrair número sem código do país (55)
                var brazilianNumber = allDigits[2..];
                
                // Validar como número brasileiro (deve ter 10-11 dígitos)
                if (brazilianNumber.Length < 10 || brazilianNumber.Length > 11)
                {
                    throw new ArgumentException(
                        $"Telefone brasileiro inválido. Após o código +55, informe DDD + número (10-11 dígitos). " +
                        $"Exemplo: +5511987654321. Recebido: +55 + {brazilianNumber.Length} dígitos");
                }
                
                ValidateBrazilianPhoneNumber(brazilianNumber);
                
                Value = brazilianNumber;
                CountryCode = "BR";
                return;
            }
            
            // Número internacional não-brasileiro (exceção)
            if (allDigits.Length < 8 || allDigits.Length > 15)
            {
                throw new ArgumentException(
                    "Número internacional inválido. Deve conter entre 8 e 15 dígitos após o '+'. " +
                    "Para números brasileiros, use +55 seguido de DDD e número");
            }
            
            Value = allDigits;
            CountryCode = "XX"; // Genérico para internacionais
            return;
        }
        
        // Validar formato do código do país (ISO 3166-1 alpha-2: duas letras)
        if (normalizedCountryCode.Length != 2 || !normalizedCountryCode.All(char.IsLetter))
        {
            throw new ArgumentException(
                $"Código do país inválido: '{countryCode}'. Deve seguir o padrão ISO 3166-1 alpha-2 (duas letras). " +
                "Exemplo: 'BR' para Brasil");
        }
        
        // Extrair apenas dígitos do valor informado
        var digitsOnly = new string(cleanValue.Where(char.IsDigit).ToArray());
        
        // VALIDAÇÃO BRASILEIRA (foco principal do sistema)
        if (normalizedCountryCode == "BR")
        {
            if (digitsOnly.Length < 10 || digitsOnly.Length > 11)
            {
                throw new ArgumentException(
                    $"Telefone brasileiro inválido. Informe DDD + número (10 dígitos para fixo ou 11 para celular). " +
                    $"Exemplo: (11) 98765-4321 ou 11987654321. Recebido: {digitsOnly.Length} dígitos");
            }
            
            ValidateBrazilianPhoneNumber(digitsOnly);
        }
        else
        {
            // Para outros países (exceção), validação genérica
            if (digitsOnly.Length < 8 || digitsOnly.Length > 15)
            {
                throw new ArgumentException(
                    $"Telefone inválido para país {normalizedCountryCode}. Deve conter entre 8 e 15 dígitos");
            }
        }
        
        Value = digitsOnly;
        CountryCode = normalizedCountryCode;
    }
    
    /// <summary>
    /// Valida regras específicas para números brasileiros.
    /// </summary>
    private static void ValidateBrazilianPhoneNumber(string digitsOnly)
    {
        // Validação DDD: primeiro dígito deve estar entre 1-9 (DDDs válidos: 11-99)
        if (digitsOnly.Length >= 2 && (digitsOnly[0] < '1' || digitsOnly[0] > '9'))
        {
            throw new ArgumentException(
                "DDD inválido. O código de área deve começar com dígito entre 1 e 9 (DDDs válidos: 11 a 99)");
        }
        
        // Validação celular: números com 11 dígitos devem ter 9 como terceiro dígito
        if (digitsOnly.Length == 11 && digitsOnly[2] != '9')
        {
            throw new ArgumentException(
                "Celular brasileiro inválido. Números com 11 dígitos devem ter 9 como terceiro dígito. " +
                "Formato esperado: (XX) 9XXXX-XXXX");
        }
        
        // Validação fixo: números com 10 dígitos NÃO devem ter 9 como terceiro dígito
        if (digitsOnly.Length == 10 && digitsOnly[2] == '9')
        {
            throw new ArgumentException(
                "Telefone fixo brasileiro inválido. Números com 10 dígitos não devem ter 9 como terceiro dígito. " +
                "Formato esperado: (XX) XXXX-XXXX");
        }
    }

    public override string ToString() => $"{CountryCode} {Value}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return CountryCode;
    }
}
