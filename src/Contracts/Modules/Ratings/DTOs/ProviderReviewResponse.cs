namespace MeAjudaAi.Contracts.Modules.Ratings.DTOs;

public record ProviderReviewResponse(Guid Id, int Rating, string? Comment, DateTime CreatedAt);
