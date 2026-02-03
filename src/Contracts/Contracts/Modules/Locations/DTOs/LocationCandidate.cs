namespace MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;

public record LocationCandidate(
    string DisplayName,
    string City,
    string State,
    string Country,
    double Latitude,
    double Longitude
);
