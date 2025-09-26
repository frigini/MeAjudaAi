using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Número de telefone.
/// </summary>
public class PhoneNumber : ValueObject
{
    public string Value { get; }
    public string CountryCode { get; }
    
    public PhoneNumber(string value, string countryCode = "BR")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Telefone não pode ser vazio");
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Código do país não pode ser vazio");
        Value = value.Trim();
        CountryCode = countryCode.Trim();
    }

    public PhoneNumber(string value) : this(value, "BR") {}

    public override string ToString() => $"{CountryCode} {Value}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return CountryCode;
    }
}