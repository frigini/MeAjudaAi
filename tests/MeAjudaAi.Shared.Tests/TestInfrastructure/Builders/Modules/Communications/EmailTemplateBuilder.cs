using MeAjudaAi.Modules.Communications.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Communications;

[ExcludeFromCodeCoverage]
public class EmailTemplateBuilder : BaseBuilder<EmailTemplate>
{
    private string? _key;
    private bool _keySet;
    private string? _subject;
    private bool _subjectSet;
    private string? _htmlBody;
    private bool _htmlBodySet;
    private string? _textBody;
    private bool _textBodySet;
    private string? _language;
    private string? _overrideKey;
    private bool _isSystemTemplate;
    private bool _isActive = true;

    public EmailTemplateBuilder()
    {
        Faker = new Faker<EmailTemplate>()
            .CustomInstantiator(f =>
            {
                var template = EmailTemplate.Create(
                    _keySet ? _key! : f.Lorem.Word().ToLowerInvariant(),
                    _subjectSet ? _subject! : f.Lorem.Sentence(3),
                    _htmlBodySet ? _htmlBody! : "<p>Hello {{Name}}</p>",
                    _textBodySet ? _textBody! : "Hello {{Name}}",
                    _language ?? "pt-BR",
                    _overrideKey,
                    _isSystemTemplate);

                if (!_isActive)
                {
                    template.Deactivate();
                }

                return template;
            });
    }

    public EmailTemplateBuilder WithKey(string key)
    {
        _key = key;
        _keySet = true;
        return this;
    }

    public EmailTemplateBuilder WithSubject(string subject)
    {
        _subject = subject;
        _subjectSet = true;
        return this;
    }

    public EmailTemplateBuilder WithHtmlBody(string htmlBody)
    {
        _htmlBody = htmlBody;
        _htmlBodySet = true;
        return this;
    }

    public EmailTemplateBuilder WithTextBody(string textBody)
    {
        _textBody = textBody;
        _textBodySet = true;
        return this;
    }

    public EmailTemplateBuilder WithLanguage(string language)
    {
        _language = language;
        return this;
    }

    public EmailTemplateBuilder WithOverrideKey(string overrideKey)
    {
        _overrideKey = overrideKey;
        return this;
    }

    public EmailTemplateBuilder AsSystemTemplate()
    {
        _isSystemTemplate = true;
        return this;
    }

    public EmailTemplateBuilder AsActive()
    {
        _isActive = true;
        return this;
    }

    public EmailTemplateBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }
}
