using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Domain.ValuleObjects;

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

    public PhoneNumber(string value) : this(value, "BR") // Default to Brazil
    {
    }

    public override string ToString() => $"{CountryCode} {Value}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return CountryCode;
    }
}