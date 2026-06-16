using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.Domain.ValueObjects;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Payments;

[ExcludeFromCodeCoverage]
public class PaymentTransactionBuilder : BaseBuilder<PaymentTransaction>
{
    private Guid? _subscriptionId;
    private Money? _amount;

    public PaymentTransactionBuilder()
    {
        Faker = new Faker<PaymentTransaction>()
            .CustomInstantiator(f =>
            {
                return new PaymentTransaction(
                    _subscriptionId ?? Guid.NewGuid(),
                    _amount ?? Money.FromDecimal(f.Random.Decimal(10, 500), "BRL")
                );
            });
    }

    public PaymentTransactionBuilder WithSubscriptionId(Guid subscriptionId)
    {
        _subscriptionId = subscriptionId;
        return this;
    }

    public PaymentTransactionBuilder WithAmount(Money amount)
    {
        _amount = amount;
        return this;
    }

    public PaymentTransactionBuilder WithAmount(decimal amount, string currency = "BRL")
    {
        _amount = Money.FromDecimal(amount, currency);
        return this;
    }

    public PaymentTransactionBuilder Settled(string externalTransactionId = "tx_ext_123")
    {
        WithCustomAction(tx => tx.Settle(externalTransactionId));
        return this;
    }

    public PaymentTransactionBuilder Failed()
    {
        WithCustomAction(tx => tx.Fail());
        return this;
    }

    public PaymentTransactionBuilder Refunded(string externalTransactionId = "tx_ext_123")
    {
        WithCustomAction(tx =>
        {
            tx.Settle(externalTransactionId);
            tx.Refund();
        });
        return this;
    }
}
