using MeAjudaAi.Shared.Geolocation;

namespace MeAjudaAi.Shared.Tests.Mocks;

/// <summary>
/// Mock implementation of IGeographicValidationService for integration tests.
/// Validates cities using simple case-insensitive string matching against allowed cities.
/// </summary>
public class MockGeographicValidationService : IGeographicValidationService
{
    private static readonly string[] DefaultCities = { "Muriaé", "Itaperuna", "Linhares" };
    private readonly HashSet<string> _allowedCities;

    /// <summary>
    /// Creates a new mock service with the default pilot cities.
    /// </summary>
    public MockGeographicValidationService()
    {
        _allowedCities = new HashSet<string>(DefaultCities, StringComparer.OrdinalIgnoreCase);
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
    /// <param name="cityName">Name of the city to validate.</param>
    /// <param name="stateSigla">Optional state abbreviation (e.g., "MG", "RJ").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<bool> ValidateCityAsync(
        string cityName,
        string? stateSigla,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            return Task.FromResult(false);

        // Simple check: city is in allowed list (case-insensitive)
        return Task.FromResult(_allowedCities.Contains(cityName));
    }

    /// <summary>
    /// Configures the mock to allow a specific city.
    /// </summary>
    /// <returns>This instance for fluent chaining.</returns>
    public MockGeographicValidationService AllowCity(string cityName)
    {
        _allowedCities.Add(cityName);
        return this;
    }

    /// <summary>
    /// Configures the mock to block a specific city.
    /// </summary>
    /// <returns>This instance for fluent chaining.</returns>
    public MockGeographicValidationService BlockCity(string cityName)
    {
        _allowedCities.Remove(cityName);
        return this;
    }

    /// <summary>
    /// Resets the mock to the default pilot cities.
    /// </summary>
    /// <returns>This instance for fluent chaining.</returns>
    public MockGeographicValidationService Reset()
    {
        _allowedCities.Clear();
        foreach (var city in DefaultCities)
        {
            _allowedCities.Add(city);
        }
        return this;
    }
}
