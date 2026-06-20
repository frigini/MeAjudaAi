using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeAjudaAi.Modules.Payments.Domain.Entities;

/// <summary>
/// Representa uma transação de pagamento associada a uma assinatura.
/// </summary>
/// <remarks>
/// Cada transação corresponde a um evento financeiro (ex: invoice.paid do Stripe).
/// Herda de BaseEntity para rastreamento de CreatedAt e UpdatedAt.
/// </remarks>
public sealed class PaymentTransaction : BaseEntity
{
    private PaymentTransaction() { }

    /// <summary>
    /// Cria uma nova transação de pagamento pendente para uma assinatura.
    /// </summary>
    /// <param name="subscriptionId">Identificador da assinatura associada.</param>
    /// <param name="amount">Valor monetário da transação.</param>
    /// <exception cref="ArgumentException">Lançada quando <paramref name="subscriptionId"/> é vazio ou <paramref name="amount"/> é zero/negativo.</exception>
    /// <exception cref="ArgumentNullException">Lançada quando <paramref name="amount"/> é null.</exception>
    public PaymentTransaction(Guid subscriptionId, Money amount)
    {
        if (subscriptionId == Guid.Empty)
            throw new ArgumentException("SubscriptionId cannot be empty.", nameof(subscriptionId));

        if (amount == null)
            throw new ArgumentNullException(nameof(amount), "Amount cannot be null.");

        if (amount.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        SubscriptionId = subscriptionId;
        Amount = amount;
        Status = EPaymentStatus.Pending;
    }

    /// <summary>
    /// Identificador da assinatura associada a esta transação.
    /// </summary>
    public Guid SubscriptionId { get; private set; }

    /// <summary>
    /// ID da transação no provedor externo (ex: ID da invoice no Stripe). Preenchido após confirmação.
    /// </summary>
    public string? ExternalTransactionId { get; private set; }

    /// <summary>
    /// Valor monetário da transação.
    /// </summary>
    public Money Amount { get; private set; } = null!;

    /// <summary>
    /// Status atual da transação (Pending, Succeeded, Failed, Refunded).
    /// </summary>
    public EPaymentStatus Status { get; private set; }

    /// <summary>
    /// Data/hora em que a transação foi processada (UTC). Null enquanto pendente.
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }
    
    /// <summary>
    /// Data/hora em que a transação foi reembolsada (UTC). Null se não reembolsada.
    /// </summary>
    [NotMapped]
    public DateTime? RefundedAt { get; private set; }

    /// <summary>
    /// Registra a confirmação de uma transação pendente.
    /// </summary>
    /// <param name="externalTransactionId">ID da transação no provedor externo.</param>
    /// <exception cref="ArgumentException">Lançada quando <paramref name="externalTransactionId"/> é nulo ou vazio.</exception>
    /// <exception cref="InvalidOperationException">Lançada quando a transação não está pendente.</exception>
    public void Settle(string externalTransactionId)
    {
        if (string.IsNullOrWhiteSpace(externalTransactionId))
            throw new ArgumentException("ExternalTransactionId cannot be empty.", nameof(externalTransactionId));

        if (Status != EPaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot settle transaction in {Status} status.");

        ExternalTransactionId = externalTransactionId;
        Status = EPaymentStatus.Succeeded;
        ProcessedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Marca uma transação pendente como falhada.
    /// </summary>
    /// <exception cref="InvalidOperationException">Lançada quando a transação não está pendente.</exception>
    public void Fail()
    {
        if (Status != EPaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot fail transaction in {Status} status.");

        Status = EPaymentStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Registra o reembolso de uma transação bem-sucedida.
    /// </summary>
    /// <exception cref="InvalidOperationException">Lançada quando a transação não está com status Succeeded.</exception>
    public void Refund()
    {
        if (Status == EPaymentStatus.Refunded) return;

        if (Status != EPaymentStatus.Succeeded)
            throw new InvalidOperationException($"Cannot refund transaction in {Status} status. Only Succeeded transactions can be refunded.");

        Status = EPaymentStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }
}
