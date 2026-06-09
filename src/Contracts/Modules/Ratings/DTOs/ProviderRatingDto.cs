namespace MeAjudaAi.Contracts.Modules.Ratings.DTOs;

public record ProviderRatingDto(
    Guid ProviderId,
    decimal AverageRating,
    int TotalReviews
);
