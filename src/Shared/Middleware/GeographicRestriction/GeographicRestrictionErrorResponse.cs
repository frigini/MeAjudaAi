using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Middleware.GeographicRestriction;

[ExcludeFromCodeCoverage]

public record GeographicRestrictionErrorResponse(
    string message,
    UserLocation? userLocation,
    IEnumerable<AllowedCity>? allowedCities,
    List<string>? allowedStates);
