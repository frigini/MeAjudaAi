using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Shared.Domain.ValueObjects;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Payments;

[ExcludeFromCodeCoverage]
public class SubscriptionBuilder : BaseBuilder<Subscription>
{
    private Guid? _id;
    private Guid? _providerId;
    private string? _planId;
    private Money? _amount;
    private ESubscriptionStatus _status = ESubscriptionStatus.Pending;
    private DateTime? _createdAt;
    private string _externalSubscriptionId = "sub_test";
    private string _externalCustomerId = "cus_test";

    public SubscriptionBuilder()
    {
        Faker = new Faker<Subscription>()
            .CustomInstantiator(f =>
            {
                return new Subscription(
                    _id ?? Guid.NewGuid(),
                    _providerId ?? Guid.NewGuid(),
                    _planId ?? f.Random.Word(),
                    _amount ?? Money.FromDecimal(f.Random.Decimal(10, 200), "BRL"),
                    _status,
                    _createdAt ?? DateTime.UtcNow
                );
            });
    }

    public SubscriptionBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public SubscriptionBuilder WithProviderId(Guid providerId)
    {
        _providerId = providerId;
        return this;
    }

    public SubscriptionBuilder WithPlanId(string planId)
    {
        _planId = planId;
        return this;
    }

    public SubscriptionBuilder WithAmount(Money amount)
    {
        _amount = amount;
        return this;
    }

    public SubscriptionBuilder WithAmount(decimal amount, string currency = "BRL")
    {
        _amount = Money.FromDecimal(amount, currency);
        return this;
    }

    public SubscriptionBuilder WithStatus(ESubscriptionStatus status)
    {
        _status = status;
        return this;
    }

    public SubscriptionBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public SubscriptionBuilder WithExternalIds(string subscriptionId, string customerId)
    {
        _externalSubscriptionId = subscriptionId;
        _externalCustomerId = customerId;
        return this;
    }

    public SubscriptionBuilder AsActive()
    {
        _status = ESubscriptionStatus.Active;
        return this;
    }

    public SubscriptionBuilder AsCanceled()
    {
        _status = ESubscriptionStatus.Canceled;
        return this;
    }

    public SubscriptionBuilder AsExpired()
    {
        _status = ESubscriptionStatus.Expired;
        return this;
    }

    public SubscriptionBuilder AsPending()
    {
        _status = ESubscriptionStatus.Pending;
        return this;
    }

    public SubscriptionBuilder Activated(DateTime? expiresAt = null)
    {
        WithCustomAction(sub => sub.Activate(_externalSubscriptionId, _externalCustomerId, expiresAt ?? DateTime.UtcNow.AddMonths(1)));
        return this;
    }

    public SubscriptionBuilder Activated(string externalSubscriptionId, string externalCustomerId, DateTime? expiresAt = null)
    {
        WithCustomAction(sub => sub.Activate(externalSubscriptionId, externalCustomerId, expiresAt ?? DateTime.UtcNow.AddMonths(1)));
        return this;
    }

    public SubscriptionBuilder Canceled()
    {
        WithCustomAction(sub => sub.Cancel());
        return this;
    }

    public SubscriptionBuilder Expired()
    {
        WithCustomAction(sub => sub.Expire());
        return this;
    }
}
