using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Value object representando um número de telefone com código do país.
/// </summary>
public class PhoneNumber : ValueObject
{
    public string Value { get; }
    public string CountryCode { get; }
    
    public PhoneNumber(string value, string countryCode = "BR")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty");
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Country code cannot be empty");
        
        Value = value.Trim();
        CountryCode = countryCode.Trim();
    }

    public PhoneNumber(string value) : this(value, "BR") // Padrão para Brasil
    {
    }

    public override string ToString() => $"{CountryCode} {Value}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return CountryCode;
    }
}