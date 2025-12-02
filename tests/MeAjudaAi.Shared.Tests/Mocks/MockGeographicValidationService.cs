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
    /// Supports both "City|State" and plain "City" formats in allowed cities list.
    /// </summary>
    /// <param name="cityName">Name of the city to validate.</param>
    /// <param name="stateSigla">Optional state abbreviation (e.g., "MG", "RJ").</param>
    /// <param name="allowedCities">List of allowed cities. If null, uses instance defaults. If empty, blocks all.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<bool> ValidateCityAsync(
        string cityName,
        string? stateSigla,
        IReadOnlyCollection<string> allowedCities,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            return Task.FromResult(false);

        // Use provided allowed cities or fall back to instance list
        // null = use defaults, empty = block all
        var citiesToCheck = allowedCities == null ? _allowedCities :
                           allowedCities.Any() ? allowedCities : [];

        // Check if city is allowed - supports "City|State" and "City" formats
        var isAllowed = citiesToCheck.Any(allowedEntry =>
        {
            // Parse allowed entry (supports "City|State" or "City")
            var parts = allowedEntry.Split('|');
            var allowedCity = parts[0].Trim();
            var allowedState = parts.Length > 1 ? parts[1].Trim() : null;

            // Match city name (case-insensitive)
            var cityMatches = string.Equals(allowedCity, cityName, StringComparison.OrdinalIgnoreCase);

            if (!cityMatches)
                return false;

            // If both have state information, validate state match
            if (!string.IsNullOrEmpty(stateSigla) && !string.IsNullOrEmpty(allowedState))
            {
                return string.Equals(allowedState, stateSigla, StringComparison.OrdinalIgnoreCase);
            }

            // If user provided state but config doesn't have it, accept (city-only match)
            // If config has state but user didn't provide it, accept (city-only match)
            // If neither has state, accept (city-only match)
            return true;
        });

        return Task.FromResult(isAllowed);
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
