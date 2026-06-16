using MeAjudaAi.Modules.Payments.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Payments;

[ExcludeFromCodeCoverage]
public class InboxMessageBuilder : BaseBuilder<InboxMessage>
{
    private string? _type;
    private string? _content;
    private string? _externalEventId;

    public InboxMessageBuilder()
    {
        Faker = new Faker<InboxMessage>()
            .CustomInstantiator(f =>
            {
                return new InboxMessage(
                    _type ?? f.PickRandom(new[] { "checkout.session.completed", "subscription.renewed", "payment.succeeded" }),
                    _content ?? $"{{\"id\": \"{f.Random.Guid()}\", \"data\": \"{f.Lorem.Sentence()}\"}}",
                    _externalEventId ?? $"evt_{f.Random.Number(1000, 9999)}"
                );
            });
    }

    public InboxMessageBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    public InboxMessageBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public InboxMessageBuilder WithExternalEventId(string externalEventId)
    {
        _externalEventId = externalEventId;
        return this;
    }

    public InboxMessageBuilder Processed()
    {
        WithCustomAction(msg => msg.MarkAsProcessed());
        return this;
    }

    public InboxMessageBuilder WithError(string error)
    {
        WithCustomAction(msg => msg.RecordError(error));
        return this;
    }

    public static InboxMessageBuilder CreateCheckoutCompleted(string? externalEventId = null)
    {
        return new InboxMessageBuilder()
            .WithType("checkout.session.completed")
            .WithContent("{\"mode\": \"subscription\", \"customer\": \"cus_123\"}")
            .WithExternalEventId(externalEventId ?? "evt_checkout_001");
    }

    public static InboxMessageBuilder CreateSubscriptionRenewed(string? externalEventId = null)
    {
        var futureDate = DateTime.UtcNow.AddMonths(1).ToString("yyyy-MM-ddTHH:mm:ssZ");
        return new InboxMessageBuilder()
            .WithType("subscription.renewed")
            .WithContent($"{{\"subscription_id\": \"sub_123\", \"expires_at\": \"{futureDate}\"}}")
            .WithExternalEventId(externalEventId ?? "evt_renewed_001");
    }

    public static InboxMessageBuilder CreateUnknown(string? externalEventId = null)
    {
        return new InboxMessageBuilder()
            .WithType("unknown.event")
            .WithContent("{\"raw\": \"data\"}")
            .WithExternalEventId(externalEventId ?? "evt_unknown_001");
    }
}
