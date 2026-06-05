namespace MeAjudaAi.Shared.Middleware.GeographicRestriction;

public record GeographicRestrictionErrorResponse(
    string message,
    UserLocation? userLocation,
    IEnumerable<AllowedCity>? allowedCities,
    List<string>? allowedStates);
