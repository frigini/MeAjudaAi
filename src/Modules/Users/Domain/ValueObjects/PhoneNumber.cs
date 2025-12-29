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
        
        // Validar comprimento (mínimo 8, máximo 15 dígitos sem formatação)
        var digitsOnly = new string(cleanValue.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length < 8 || digitsOnly.Length > 15)
            throw new ArgumentException("Telefone deve ter entre 8 e 15 dígitos");
        
        // Validar formato para Brasil: aceita (XX) XXXXX-XXXX, (XX) XXXX-XXXX, ou apenas dígitos
        if (countryCode.Trim().Equals("BR", StringComparison.OrdinalIgnoreCase))
        {
            if (digitsOnly.Length < 10 || digitsOnly.Length > 11)
                throw new ArgumentException("Telefone brasileiro deve ter 10 ou 11 dígitos (DDD + número)");
        }
        
        Value = cleanValue;
        CountryCode = countryCode.Trim();
    }

    public override string ToString() => $"{CountryCode} {Value}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return CountryCode;
    }
}
