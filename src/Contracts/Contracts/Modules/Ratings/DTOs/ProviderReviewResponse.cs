namespace MeAjudaAi.Contracts.Contracts.Modules.Ratings.DTOs;

public record ProviderReviewResponse(Guid Id, int Rating, string? Comment, DateTime CreatedAt);
