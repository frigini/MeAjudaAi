using MeAjudaAi.Modules.Communications.Application.DTOs;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.DTOs;

[Trait("Category", "Unit")]
public class EmailOutboxPayloadTests
{
    [Fact]
    public void Create_WithHtmlBody_ShouldReturnPayloadWithHtmlBody()
    {
        var payload = EmailOutboxPayload.Create("to@test.com", "Subject", htmlBody: "<p>Hello</p>");

        payload.To.Should().Be("to@test.com");
        payload.Subject.Should().Be("Subject");
        payload.HtmlBody.Should().Be("<p>Hello</p>");
        payload.TextBody.Should().BeNull();
        payload.TemplateKey.Should().BeNull();
    }

    [Fact]
    public void Create_WithTextBody_ShouldReturnPayloadWithTextBody()
    {
        var payload = EmailOutboxPayload.Create("to@test.com", "Subject", textBody: "Hello");

        payload.TextBody.Should().Be("Hello");
        payload.HtmlBody.Should().BeNull();
        payload.TemplateKey.Should().BeNull();
    }

    [Fact]
    public void Create_WithTemplateKey_ShouldReturnPayloadWithTemplateKey()
    {
        var payload = EmailOutboxPayload.Create("to@test.com", "Subject", templateKey: "welcome");

        payload.TemplateKey.Should().Be("welcome");
        payload.HtmlBody.Should().BeNull();
        payload.TextBody.Should().BeNull();
        payload.TemplateData.Should().BeNull();
    }

    [Fact]
    public void Create_WithTemplateKeyAndTemplateData_ShouldReturnPayloadWithBoth()
    {
        var data = new Dictionary<string, string> { ["name"] = "Alice" };

        var payload = EmailOutboxPayload.Create("to@test.com", "Subject", templateKey: "welcome", templateData: data);

        payload.TemplateKey.Should().Be("welcome");
        payload.TemplateData.Should().ContainKey("name").WhoseValue.Should().Be("Alice");
    }

    [Fact]
    public void Create_WithFromAddress_ShouldSetFromField()
    {
        var payload = EmailOutboxPayload.Create("to@test.com", "Subject", htmlBody: "<p>Hi</p>", from: "sender@test.com");

        payload.From.Should().Be("sender@test.com");
    }

    [Fact]
    public void Create_WithTemplateKeyAndHtmlBody_ShouldThrowArgumentException()
    {
        var act = () => EmailOutboxPayload.Create("to@test.com", "Subject", htmlBody: "<p>Hi</p>", templateKey: "welcome");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithTemplateKeyAndTextBody_ShouldThrowArgumentException()
    {
        var act = () => EmailOutboxPayload.Create("to@test.com", "Subject", textBody: "Hi", templateKey: "welcome");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithBothHtmlBodyAndTextBody_ShouldThrowArgumentException()
    {
        var act = () => EmailOutboxPayload.Create("to@test.com", "Subject", htmlBody: "<p>Hi</p>", textBody: "Hi");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithTemplateDataButNoTemplateKey_ShouldThrowArgumentException()
    {
        var data = new Dictionary<string, string> { ["name"] = "Alice" };

        var act = () => EmailOutboxPayload.Create("to@test.com", "Subject", templateData: data);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyTemplateDataAndNoTemplateKey_ShouldSucceed()
    {
        var emptyData = new Dictionary<string, string>();

        var payload = EmailOutboxPayload.Create("to@test.com", "Subject", htmlBody: "<p>Hi</p>", templateData: emptyData);

        payload.Should().NotBeNull();
    }
}