namespace MeAjudaAi.Contracts.Contracts.Modules.Ratings.DTOs;

public record CreateReviewRequest(Guid ProviderId, int Rating, string? Comment);
