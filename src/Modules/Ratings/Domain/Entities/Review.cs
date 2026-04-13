using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.Events;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain;

namespace MeAjudaAi.Modules.Ratings.Domain.Entities;

/// <summary>
/// Representa uma avaliação de um prestador de serviços feita por um cliente.
/// </summary>
public sealed class Review : AggregateRoot<ReviewId>
{
    /// <summary>
    /// ID do prestador avaliado.
    /// </summary>
    public Guid ProviderId { get; private set; }

    /// <summary>
    /// ID do cliente que avaliou.
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// Nota da avaliação (1 a 5 estrelas).
    /// </summary>
    public int Rating { get; private set; }

    /// <summary>
    /// Comentário textual da avaliação.
    /// </summary>
    public string? Comment { get; private set; }

    /// <summary>
    /// Status atual da avaliação para moderação.
    /// </summary>
    public EReviewStatus Status { get; private set; }

    /// <summary>
    /// Motivo de rejeição (se aplicável).
    /// </summary>
    public string? RejectionReason { get; private set; }

    private Review() { }

    private Review(ReviewId id, Guid providerId, Guid customerId, int rating, string? comment) : base(id)
    {
        ProviderId = providerId;
        CustomerId = customerId;
        Rating = rating;
        Comment = comment;
        Status = EReviewStatus.Pending;

        AddDomainEvent(new ReviewCreatedDomainEvent(
            Id,
            0,
            ProviderId,
            CustomerId,
            Rating,
            Comment));
    }

    /// <summary>
    /// Cria uma nova avaliação.
    /// </summary>
    public static Review Create(Guid providerId, Guid customerId, int rating, string? comment)
    {
        if (providerId == Guid.Empty) throw new ArgumentException("ProviderId não pode ser vazio", nameof(providerId));
        if (customerId == Guid.Empty) throw new ArgumentException("CustomerId não pode ser vazio", nameof(customerId));
        if (rating < 1 || rating > 5) throw new ArgumentOutOfRangeException(nameof(rating), "Rating deve ser entre 1 e 5");

        return new Review(ReviewId.New(), providerId, customerId, rating, comment);
    }

    /// <summary>
    /// Aprova a avaliação tornando-a pública.
    /// </summary>
    public void Approve()
    {
        if (Status == EReviewStatus.Approved) return;

        Status = EReviewStatus.Approved;
        MarkAsUpdated();
        
        AddDomainEvent(new ReviewApprovedDomainEvent(
            Id,
            0, // Versioning is simple for now
            ProviderId,
            Rating,
            Comment));
    }

    /// <summary>
    /// Rejeita a avaliação por violação de regras.
    /// </summary>
    public void Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Motivo de rejeição é obrigatório", nameof(reason));
        if (Status == EReviewStatus.Rejected) return;

        Status = EReviewStatus.Rejected;
        RejectionReason = reason;
        MarkAsUpdated();
    }
}
