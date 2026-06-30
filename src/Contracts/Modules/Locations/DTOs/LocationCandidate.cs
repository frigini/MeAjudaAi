namespace MeAjudaAi.Contracts.Modules.Locations.DTOs;

public sealed record LocationCandidate(
    string DisplayName,
    string City,
    string State,
    string Country,
    double Latitude,
    double Longitude
);
