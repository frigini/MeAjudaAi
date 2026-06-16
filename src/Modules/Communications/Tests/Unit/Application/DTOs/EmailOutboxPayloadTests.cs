using MeAjudaAi.Modules.Communications.Application.DTOs;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.DTOs;

[Trait("Category", "Unit")]
[Trait("Module", "Communications")]
[Trait("Layer", "Application")]
public class EmailOutboxPayloadTests
{
    [Fact]
    public void Create_WithHtmlBody_ShouldSucceed()
    {
        // Act
        var payload = EmailOutboxPayload.Create(
            to: "user@test.com",
            subject: "Hello",
            htmlBody: "<h1>Hello</h1>");

        // Assert
        payload.To.Should().Be("user@test.com");
        payload.Subject.Should().Be("Hello");
        payload.HtmlBody.Should().Be("<h1>Hello</h1>");
        payload.TextBody.Should().BeNull();
        payload.TemplateKey.Should().BeNull();
        payload.TemplateData.Should().BeNull();
    }

    [Fact]
    public void Create_WithTextBody_ShouldSucceed()
    {
        // Act
        var payload = EmailOutboxPayload.Create(
            to: "user@test.com",
            subject: "Hello",
            textBody: "Hello plain text");

        // Assert
        payload.TextBody.Should().Be("Hello plain text");
        payload.HtmlBody.Should().BeNull();
        payload.TemplateKey.Should().BeNull();
    }

    [Fact]
    public void Create_WithTemplateKey_ShouldSucceed()
    {
        // Arrange
        var templateData = new Dictionary<string, string> { ["Name"] = "John" };

        // Act
        var payload = EmailOutboxPayload.Create(
            to: "user@test.com",
            subject: "Hello",
            templateKey: "welcome",
            templateData: templateData);

        // Assert
        payload.TemplateKey.Should().Be("welcome");
        payload.TemplateData.Should().ContainKey("Name").WhoseValue.Should().Be("John");
        payload.HtmlBody.Should().BeNull();
        payload.TextBody.Should().BeNull();
    }

    [Fact]
    public void Create_WithTemplateKeyOnly_NoTemplateData_ShouldSucceed()
    {
        // Act
        var payload = EmailOutboxPayload.Create(
            to: "user@test.com",
            subject: "Hello",
            templateKey: "welcome");

        // Assert
        payload.TemplateKey.Should().Be("welcome");
        payload.TemplateData.Should().BeNull();
    }

    [Fact]
    public void Create_WithFromAddress_ShouldSetFrom()
    {
        // Act
        var payload = EmailOutboxPayload.Create(
            to: "user@test.com",
            subject: "Hello",
            htmlBody: "<p>Hello</p>",
            from: "noreply@system.com");

        // Assert
        payload.From.Should().Be("noreply@system.com");
    }

    [Fact]
    public void Create_WhenTemplateKeyAndHtmlBodyCombined_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act = () => EmailOutboxPayload.Create(
            to: "user@test.com",
            subject: "Hello",
            htmlBody: "<h1>Hello</h1>",
            templateKey: "welcome");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*TemplateKey/TemplateData não pode ser combinado com HtmlBody ou TextBody*");
    }

    [Fact]
    public void Create_WhenTemplateKeyAndTextBodyCombined_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act = () => EmailOutboxPayload.Create(
            to: "user@test.com",
            subject: "Hello",
            textBody: "Hello text",
            templateKey: "welcome");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*TemplateKey/TemplateData não pode ser combinado com HtmlBody ou TextBody*");
    }

    [Fact]
    public void Create_WhenHtmlBodyAndTextBodyBothSet_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act = () => EmailOutboxPayload.Create(
            to: "user@test.com",
            subject: "Hello",
            htmlBody: "<h1>Hello</h1>",
            textBody: "Hello text");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*HtmlBody e TextBody não podem ser definidos simultaneamente*");
    }

    [Fact]
    public void Create_WhenTemplateDataWithoutTemplateKey_ShouldThrowArgumentException()
    {
        // Arrange
        var templateData = new Dictionary<string, string> { ["Name"] = "John" };

        // Act & Assert
        var act = () => EmailOutboxPayload.Create(
            to: "user@test.com",
            subject: "Hello",
            templateData: templateData);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*TemplateData requer que TemplateKey seja informado*");
    }

    [Fact]
    public void Create_WithEmptyTemplateData_AndNoTemplateKey_ShouldSucceed()
    {
        // Empty dictionary is not > 0 so should not throw
        var emptyData = new Dictionary<string, string>();

        // Act
        var payload = EmailOutboxPayload.Create(
            to: "user@test.com",
            subject: "Hello",
            htmlBody: "<p>Hi</p>",
            templateData: emptyData);

        // Assert
        payload.HtmlBody.Should().Be("<p>Hi</p>");
        payload.TemplateData.Should().BeNull();
    }
}
