using MeAjudaAi.Modules.Communications.Application.DTOs;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Communications;

[ExcludeFromCodeCoverage]
public class EmailOutboxPayloadBuilder : BaseBuilder<EmailOutboxPayload>
{
    private string? _to;
    private string? _subject;
    private string? _htmlBody;
    private string? _textBody;
    private string? _from;
    private string? _templateKey;
    private IDictionary<string, string>? _templateData;

    public EmailOutboxPayloadBuilder()
    {
        Faker = new Faker<EmailOutboxPayload>()
            .CustomInstantiator(f =>
            {
                return EmailOutboxPayload.Create(
                    _to ?? f.Internet.Email(),
                    _subject ?? f.Lorem.Sentence(3),
                    _htmlBody,
                    _textBody,
                    _from,
                    _templateKey,
                    _templateData == null ? null : new Dictionary<string, string>(_templateData));
            });
    }

    public EmailOutboxPayloadBuilder WithTo(string to)
    {
        _to = to;
        return this;
    }

    public EmailOutboxPayloadBuilder WithSubject(string subject)
    {
        _subject = subject;
        return this;
    }

    public EmailOutboxPayloadBuilder WithHtmlBody(string htmlBody)
    {
        _htmlBody = htmlBody;
        return this;
    }

    public EmailOutboxPayloadBuilder WithTextBody(string textBody)
    {
        _textBody = textBody;
        return this;
    }

    public EmailOutboxPayloadBuilder WithFrom(string from)
    {
        _from = from;
        return this;
    }

    public EmailOutboxPayloadBuilder AsTemplate(string templateKey, IDictionary<string, string>? data = null)
    {
        _templateKey = templateKey;
        _templateData = data;
        return this;
    }
}
