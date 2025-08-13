using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Domain.ValuleObjects;

public record PhoneNumber : ValueObject
{
    public string Number { get; }
    public string CountryCode { get; }
    public PhoneNumber(string number, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Phone number cannot be empty");
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("Country code cannot be empty");
        Number = number.Trim();
        CountryCode = countryCode.Trim();
    }

    public PhoneNumber(string number) : this(number, "BR") // Default to Brazil
    {
    }

    public PhoneNumber() : this(string.Empty, "BR") // Default to Brazil with empty number
    {
    }

    public PhoneNumber(string number, int countryCode) : this(number, countryCode.ToString())
    {
    }

    public override string ToString() => $"{CountryCode} {Number}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Number;
        yield return CountryCode;
    }
}