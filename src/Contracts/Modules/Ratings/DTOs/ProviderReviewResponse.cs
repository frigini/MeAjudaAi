namespace MeAjudaAi.Contracts.Modules.Ratings.DTOs;

public sealed record ProviderReviewResponse(Guid Id, int Rating, string? Comment, DateTime CreatedAt);
