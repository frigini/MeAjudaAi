using MeAjudaAi.Shared.Geolocation;

namespace MeAjudaAi.Modules.Location.Domain.ValueObjects;

/// <summary>
/// Representa um endereço brasileiro completo.
/// </summary>
public sealed class Address
{
    private static readonly HashSet<string> ValidUfs = new(StringComparer.OrdinalIgnoreCase)
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA",
        "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN",
        "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    public Cep Cep { get; }
    public string Street { get; }
    public string Neighborhood { get; }
    public string City { get; }
    public string State { get; }
    public string? Complement { get; }
    public GeoPoint? GeoPoint { get; }

    private Address(
        Cep cep,
        string street,
        string neighborhood,
        string city,
        string state,
        string? complement = null,
        GeoPoint? geoPoint = null)
    {
        Cep = cep;
        Street = street;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        Complement = complement;
        GeoPoint = geoPoint;
    }

    public static Address? Create(
        Cep? cep,
        string? street,
        string? neighborhood,
        string? city,
        string? state,
        string? complement = null,
        GeoPoint? geoPoint = null)
    {
        if (cep is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(street))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(neighborhood))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            return null;
        }

        // Validar UF (2 letras e código válido)
        var upperState = state.ToUpperInvariant();
        if (!ValidUfs.Contains(upperState))
        {
            return null;
        }

        return new Address(
            cep,
            street.Trim(),
            neighborhood.Trim(),
            city.Trim(),
            state.ToUpperInvariant(),
            string.IsNullOrWhiteSpace(complement) ? null : complement.Trim(),
            geoPoint);
    }

    public override string ToString()
    {
        var parts = new List<string> { Street, Neighborhood, City, State, Cep.Formatted };
        
        if (!string.IsNullOrWhiteSpace(Complement))
        {
            parts.Insert(1, Complement);
        }

        return string.Join(", ", parts);
    }
}
