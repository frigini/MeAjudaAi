using MeAjudaAi.Modules.Communications.Application.DTOs;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Communications;

[ExcludeFromCodeCoverage]
public class PushOutboxPayloadBuilder : BaseBuilder<PushOutboxPayload>
{
    private string? _deviceToken;
    private string? _title;
    private string? _body;
    private IDictionary<string, string>? _data;

    public PushOutboxPayloadBuilder()
    {
        Faker = new Faker<PushOutboxPayload>()
            .CustomInstantiator(f =>
            {
                return new PushOutboxPayload(
                    _deviceToken ?? f.Random.String(32),
                    _title ?? f.Lorem.Sentence(3),
                    _body ?? f.Lorem.Sentence(),
                    _data);
            });
    }

    public PushOutboxPayloadBuilder WithDeviceToken(string token)
    {
        _deviceToken = token;
        return this;
    }

    public PushOutboxPayloadBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public PushOutboxPayloadBuilder WithBody(string body)
    {
        _body = body;
        return this;
    }

    public PushOutboxPayloadBuilder WithData(IDictionary<string, string> data)
    {
        _data = data;
        return this;
    }
}
