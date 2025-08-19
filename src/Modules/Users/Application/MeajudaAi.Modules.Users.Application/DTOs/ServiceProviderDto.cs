namespace MeAjudaAi.Modules.Users.Application.DTOs;

public sealed record ServiceProviderDto(
    Guid Id,
    Guid UserId,
    string CompanyName,
    string? TaxId,
    string Tier,
    string SubscriptionStatus,
    DateTime? SubscriptionExpiresAt,
    string? SubscriptionId,
    List<string> ServiceCategories,
    string? Description,
    decimal Rating,
    int TotalReviews,
    bool IsVerified,
    DateTime? VerifiedAt,
    int MaxActiveServices,
    bool CanAccessPremiumFeatures,
    bool CanCustomizeBranding,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);