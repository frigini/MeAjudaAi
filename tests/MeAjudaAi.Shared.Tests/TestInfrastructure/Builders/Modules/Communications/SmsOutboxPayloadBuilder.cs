using MeAjudaAi.Modules.Communications.Application.DTOs;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Communications;

[ExcludeFromCodeCoverage]
public class SmsOutboxPayloadBuilder : BaseBuilder<SmsOutboxPayload>
{
    private string? _phoneNumber;
    private string? _body;

    public SmsOutboxPayloadBuilder()
    {
        Faker = new Faker<SmsOutboxPayload>()
            .CustomInstantiator(f =>
            {
                return new SmsOutboxPayload(
                    _phoneNumber ?? $"+55{f.Random.Number(11, 99)}{f.Random.Number(900000000, 999999999)}",
                    _body ?? f.Lorem.Sentence());
            });
    }

    public SmsOutboxPayloadBuilder WithPhoneNumber(string phoneNumber)
    {
        _phoneNumber = phoneNumber;
        return this;
    }

    public SmsOutboxPayloadBuilder WithBody(string body)
    {
        _body = body;
        return this;
    }
}
