using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Communications;

[ExcludeFromCodeCoverage]
public class OutboxMessageBuilder : BaseBuilder<OutboxMessage>
{
    private ECommunicationChannel? _channel;
    private string? _payload;
    private bool _payloadSet;
    private ECommunicationPriority _priority = ECommunicationPriority.Normal;
    private DateTime? _scheduledAt;
    private int _maxRetries = 3;
    private string? _correlationId;

    public OutboxMessageBuilder()
    {
        Faker = new Faker<OutboxMessage>()
            .CustomInstantiator(f =>
            {
                return OutboxMessage.Create(
                    _channel ?? ECommunicationChannel.Email,
                    _payloadSet ? _payload! : "{}",
                    _priority,
                    _scheduledAt,
                    _maxRetries,
                    _correlationId);
            });
    }

    public OutboxMessageBuilder WithChannel(ECommunicationChannel channel)
    {
        _channel = channel;
        return this;
    }

    public OutboxMessageBuilder WithPayload(string payload)
    {
        _payload = payload;
        _payloadSet = true;
        return this;
    }

    public OutboxMessageBuilder WithPriority(ECommunicationPriority priority)
    {
        _priority = priority;
        return this;
    }

    public OutboxMessageBuilder WithMaxRetries(int maxRetries)
    {
        _maxRetries = maxRetries;
        return this;
    }

    public OutboxMessageBuilder AsScheduled(DateTime scheduledAt)
    {
        _scheduledAt = scheduledAt;
        return this;
    }

    public OutboxMessageBuilder WithCorrelationId(string correlationId)
    {
        _correlationId = correlationId;
        return this;
    }
}
