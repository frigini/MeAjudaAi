using MeAjudaAi.Shared.Events;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Messaging.Messages.Ratings;

/// <summary>
/// Evento de integração disparado quando uma nova avaliação é aprovada e incorporada à média do prestador.
/// </summary>
[ExcludeFromCodeCoverage]
public record ReviewApprovedIntegrationEvent(
    string Source,
    Guid ProviderId,
    Guid ReviewId,
    decimal NewAverageRating,
    int TotalReviews,
    int ReviewRating,
    string? ReviewComment,
    DateTime CreatedAt
) : IntegrationEvent(Source);
