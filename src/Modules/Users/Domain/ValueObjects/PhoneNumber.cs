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
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Código do país não pode ser vazio");
        
        var cleanValue = value.Trim();
        var cleanCountryCode = countryCode.Trim().ToUpperInvariant();
        
        // Validar formato do código do país (ISO alpha-2: exatamente 2 letras ASCII)
        if (cleanCountryCode.Length != 2 || !cleanCountryCode.All(c => c >= 'A' && c <= 'Z'))
            throw new ArgumentException($"Código do país '{countryCode}' inválido. Deve ser um código ISO alpha-2 (2 letras)");
        
        // Validar comprimento (apenas dígitos)
        var digitsOnly = new string(cleanValue.Where(char.IsDigit).ToArray());
        
        // Validação específica por país
        if (cleanCountryCode == "BR")
        {
            // Brasil: 10-11 dígitos (2 DDD + 8-9 subscriber)
            if (digitsOnly.Length < 10 || digitsOnly.Length > 11)
                throw new ArgumentException("Telefone brasileiro deve ter 10 ou 11 dígitos (DDD + número)");
        }
        else
        {
            // Outros países: 8-15 dígitos
            if (digitsOnly.Length < 8)
                throw new ArgumentException("Telefone deve ter pelo menos 8 dígitos");
            if (digitsOnly.Length > 15)
                throw new ArgumentException("Telefone não pode ter mais de 15 dígitos");
        }
        
        // Armazenar apenas dígitos normalizados para consistência de igualdade
        Value = digitsOnly;
        CountryCode = cleanCountryCode;
    }

    public override string ToString() => $"{CountryCode} {Value}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return CountryCode;
    }
}
