using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Communications;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Domain.Entities;

public class EmailTemplateTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateTemplate()
    {
        // Act
        var template = new EmailTemplateBuilder()
            .WithKey("user_registered")
            .WithSubject("Welcome")
            .WithHtmlBody("<h1>Hello</h1>")
            .WithTextBody("Hello")
            .WithLanguage("en-US")
            .WithOverrideKey("custom-key")
            .AsSystemTemplate()
            .AsActive()
            .Build();

        // Assert
        template.TemplateKey.Should().Be("user_registered");
        template.Subject.Should().Be("Welcome");
        template.HtmlBody.Should().Be("<h1>Hello</h1>");
        template.TextBody.Should().Be("Hello");
        template.Language.Should().Be("en-us");
        template.OverrideKey.Should().Be("custom-key");
        template.IsSystemTemplate.Should().BeTrue();
        template.IsActive.Should().BeTrue();
        template.Version.Should().Be(1);
    }

    [Fact]
    public void CreateNewVersion_WhenNotSystemTemplate_ShouldCreateNewVersion()
    {
        // Arrange
        var template = new EmailTemplateBuilder()
            .WithKey("test")
            .WithSubject("S")
            .WithHtmlBody("H")
            .WithTextBody("T")
            .Build();

        // Act
        var newVersion = template.CreateNewVersion("New S", "New H", "New T");

        // Assert
        newVersion.Should().NotBeSameAs(template);
        newVersion.Subject.Should().Be("New S");
        newVersion.HtmlBody.Should().Be("New H");
        newVersion.TextBody.Should().Be("New T");
        newVersion.Version.Should().Be(2);
        newVersion.TemplateKey.Should().Be(template.TemplateKey);
        newVersion.Language.Should().Be(template.Language);
        newVersion.OverrideKey.Should().Be(template.OverrideKey);
        newVersion.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateNewVersion_WhenSystemTemplate_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var template = new EmailTemplateBuilder()
            .WithKey("test")
            .WithSubject("S")
            .WithHtmlBody("H")
            .WithTextBody("T")
            .AsSystemTemplate()
            .Build();

        // Act
        var act = () => template.CreateNewVersion("New S", "New H", "New T");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Deactivate_WhenNotSystemTemplate_ShouldDeactivate()
    {
        // Arrange
        var template = new EmailTemplateBuilder()
            .WithKey("test")
            .WithSubject("S")
            .WithHtmlBody("H")
            .WithTextBody("T")
            .Build();

        // Act
        template.Deactivate();

        // Assert
        template.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenSystemTemplate_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var template = new EmailTemplateBuilder()
            .WithKey("test")
            .WithSubject("S")
            .WithHtmlBody("H")
            .WithTextBody("T")
            .AsSystemTemplate()
            .Build();

        // Act
        var act = () => template.Deactivate();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Activate_ShouldActivate()
    {
        // Arrange
        var template = new EmailTemplateBuilder()
            .WithKey("test")
            .WithSubject("S")
            .WithHtmlBody("H")
            .WithTextBody("T")
            .Build();
        template.Deactivate();

        // Act
        template.Activate();

        // Assert
        template.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null, "S", "H", "T")]
    [InlineData("", "S", "H", "T")]
    [InlineData(" ", "S", "H", "T")]
    [InlineData("K", null, "H", "T")]
    [InlineData("K", "S", null, "T")]
    [InlineData("K", "S", "H", null)]
    public void Create_WithInvalidData_ShouldThrowArgumentException(string? key, string? sub, string? html, string? text)
    {
        // Act
        var act = () => new EmailTemplateBuilder().WithKey(key!).WithSubject(sub!).WithHtmlBody(html!).WithTextBody(text!).Build();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null, "H", "T")]
    [InlineData("S", null, "T")]
    [InlineData("S", "H", null)]
    public void CreateNewVersion_WithInvalidData_ShouldThrowArgumentException(string? sub, string? html, string? text)
    {
        // Arrange
        var template = new EmailTemplateBuilder()
            .WithKey("test")
            .WithSubject("S")
            .WithHtmlBody("H")
            .WithTextBody("T")
            .Build();

        // Act
        var act = () => template.CreateNewVersion(sub!, html!, text!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
