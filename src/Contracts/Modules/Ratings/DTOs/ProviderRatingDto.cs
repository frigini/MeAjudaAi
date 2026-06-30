namespace MeAjudaAi.Contracts.Modules.Ratings.DTOs;

public sealed record ProviderRatingDto(
    Guid ProviderId,
    decimal AverageRating,
    int TotalReviews
);
