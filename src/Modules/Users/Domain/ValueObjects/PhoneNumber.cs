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
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    private PhoneNumber()
    {
    }
    #pragma warning restore CS8618

    public PhoneNumber(string value, string countryCode = "BR")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Telefone não pode ser vazio");
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Código do país não pode ser vazio");
        
        var cleanValue = value.Trim();
        
        // Validar comprimento mínimo e máximo (apenas dígitos)
        var digitsOnly = new string(cleanValue.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length < 8)
            throw new ArgumentException("Telefone deve ter pelo menos 8 dígitos");
        if (digitsOnly.Length > 15)
            throw new ArgumentException("Telefone não pode ter mais de 15 dígitos");
        
        // Armazenar apenas dígitos normalizados para consistência de igualdade
        Value = digitsOnly;
        CountryCode = countryCode.Trim().ToUpperInvariant();
    }

    public override string ToString() => $"{CountryCode} {Value}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return CountryCode;
    }
}
