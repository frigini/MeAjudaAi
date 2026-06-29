namespace MeAjudaAi.Contracts.Modules.Ratings.DTOs;

public sealed record CreateReviewRequest(Guid ProviderId, int Rating, string? Comment);
