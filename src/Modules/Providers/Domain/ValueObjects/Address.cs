using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Providers.Domain.ValueObjects;

/// <summary>
/// Endereço do prestador de serviços.
/// </summary>
public class Address : ValueObject
{
    public string Street { get; private set; }
    public string Number { get; private set; }
    public string? Complement { get; private set; }
    public string Neighborhood { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }

    /// <summary>
    /// Construtor privado para Entity Framework
    /// </summary>
    private Address()
    {
        Street = string.Empty;
        Number = string.Empty;
        Neighborhood = string.Empty;
        City = string.Empty;
        State = string.Empty;
        ZipCode = string.Empty;
        Country = string.Empty;
    }

    public Address(
        string street,
        string number,
        string neighborhood,
        string city,
        string state,
        string zipCode,
        string country = "Brazil",
        string? complement = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Rua não pode ser vazia", nameof(street));
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Número não pode ser vazio", nameof(number));
        if (string.IsNullOrWhiteSpace(neighborhood))
            throw new ArgumentException("Bairro não pode ser vazio", nameof(neighborhood));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("Cidade não pode ser vazia", nameof(city));
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("Estado não pode ser vazio", nameof(state));
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("CEP não pode ser vazio", nameof(zipCode));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("País não pode ser vazio", nameof(country));

        Street = street.Trim();
        Number = number.Trim();
        Complement = complement?.Trim();
        Neighborhood = neighborhood.Trim();
        City = city.Trim();
        State = state.Trim();
        ZipCode = zipCode.Trim();
        Country = country.Trim();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return Number;
        yield return Complement ?? string.Empty;
        yield return Neighborhood;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }

    public override string ToString() =>
        $"{Street}, {Number}{(string.IsNullOrEmpty(Complement) ? "" : ", " + Complement)}, {Neighborhood}, {City}/{State}, {ZipCode}, {Country}";
}
