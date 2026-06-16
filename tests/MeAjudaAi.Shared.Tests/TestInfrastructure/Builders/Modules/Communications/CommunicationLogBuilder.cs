using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Communications;

[ExcludeFromCodeCoverage]
public class CommunicationLogBuilder : BaseBuilder<CommunicationLog>
{
    private string? _correlationId;
    private bool _correlationIdSet;
    private ECommunicationChannel? _channel;
    private string? _recipient;
    private bool _recipientSet;
    private bool _isSuccess = true;
    private string? _errorMessage;
    private int _attemptCount = 1;
    private Guid? _outboxMessageId;
    private string? _templateKey;

    public CommunicationLogBuilder()
    {
        Faker = new Faker<CommunicationLog>()
            .CustomInstantiator(f =>
            {
                var correlationId = _correlationIdSet ? _correlationId! : $"correlation_{f.Random.Number(1000, 9999)}";
                var channel = _channel ?? ECommunicationChannel.Email;
                var recipient = _recipientSet ? _recipient! : f.Internet.Email();

                if (_isSuccess)
                {
                    return CommunicationLog.CreateSuccess(
                        correlationId,
                        channel,
                        recipient,
                        _attemptCount,
                        _outboxMessageId,
                        _templateKey);
                }
                else
                {
                    return CommunicationLog.CreateFailure(
                        correlationId,
                        channel,
                        recipient,
                        _errorMessage!,
                        _attemptCount,
                        _outboxMessageId,
                        _templateKey);
                }
            });
    }

    public CommunicationLogBuilder WithCorrelationId(string correlationId)
    {
        _correlationId = correlationId;
        _correlationIdSet = true;
        return this;
    }

    public CommunicationLogBuilder WithChannel(ECommunicationChannel channel)
    {
        _channel = channel;
        return this;
    }

    public CommunicationLogBuilder WithRecipient(string recipient)
    {
        _recipient = recipient;
        _recipientSet = true;
        return this;
    }

    public CommunicationLogBuilder AsSuccess()
    {
        _isSuccess = true;
        return this;
    }

    public CommunicationLogBuilder AsFailure(string errorMessage)
    {
        _isSuccess = false;
        _errorMessage = errorMessage;
        return this;
    }

    public CommunicationLogBuilder WithAttemptCount(int count)
    {
        _attemptCount = count;
        return this;
    }

    public CommunicationLogBuilder WithOutboxMessageId(Guid id)
    {
        _outboxMessageId = id;
        return this;
    }

    public CommunicationLogBuilder WithTemplateKey(string key)
    {
        _templateKey = key;
        return this;
    }
}
