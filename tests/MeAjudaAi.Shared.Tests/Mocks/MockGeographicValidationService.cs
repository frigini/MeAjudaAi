using MeAjudaAi.Shared.Geolocation;

namespace MeAjudaAi.Shared.Tests.Mocks;

/// <summary>
/// Mock implementation of IGeographicValidationService for integration tests.
/// Validates cities using simple case-insensitive string matching against allowed cities.
/// </summary>
public class MockGeographicValidationService : IGeographicValidationService
{
    private readonly HashSet<string> _allowedCities;

    /// <summary>
    /// Creates a new mock service with the default pilot cities.
    /// </summary>
    public MockGeographicValidationService()
    {
        _allowedCities = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Muriaé",
            "Itaperuna",
            "Linhares"
        };
    }

    /// <summary>
    /// Creates a new mock service with custom allowed cities.
    /// </summary>
    public MockGeographicValidationService(IEnumerable<string> allowedCities)
    {
        _allowedCities = new HashSet<string>(allowedCities, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates if a city is in the allowed list using case-insensitive matching.
    /// Simplified implementation for testing - does not call IBGE API.
    /// </summary>
    public Task<bool> ValidateCityAsync(
        string cityName,
        string? stateSigla,
        IReadOnlyCollection<string> allowedCities,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            return Task.FromResult(false);

        // Use provided allowed cities or fall back to instance list
        var citiesToCheck = allowedCities?.Any() == true
            ? new HashSet<string>(allowedCities, StringComparer.OrdinalIgnoreCase)
            : _allowedCities;

        var isAllowed = citiesToCheck.Contains(cityName);
        return Task.FromResult(isAllowed);
    }

    /// <summary>
    /// Configures the mock to allow a specific city.
    /// </summary>
    public void AllowCity(string cityName)
    {
        _allowedCities.Add(cityName);
    }

    /// <summary>
    /// Configures the mock to block a specific city.
    /// </summary>
    public void BlockCity(string cityName)
    {
        _allowedCities.Remove(cityName);
    }

    /// <summary>
    /// Resets the mock to the default pilot cities.
    /// </summary>
    public void Reset()
    {
        _allowedCities.Clear();
        _allowedCities.Add("Muriaé");
        _allowedCities.Add("Itaperuna");
        _allowedCities.Add("Linhares");
    }
}
