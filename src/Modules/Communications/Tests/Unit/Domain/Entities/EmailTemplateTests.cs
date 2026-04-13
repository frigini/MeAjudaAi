using MeAjudaAi.Modules.Communications.Domain.Entities;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Domain.Entities;

public class EmailTemplateTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateTemplate()
    {
        // Act
        var template = EmailTemplate.Create(
            "user_registered",
            "Welcome",
            "<h1>Hello</h1>",
            "Hello",
            "en-US",
            "custom-key",
            true);

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
    public void UpdateContent_WhenNotSystemTemplate_ShouldUpdate()
    {
        // Arrange
        var template = EmailTemplate.Create("test", "S", "H", "T", isSystemTemplate: false);

        // Act
        template.UpdateContent("New S", "New H", "New T");

        // Assert
        template.Subject.Should().Be("New S");
        template.HtmlBody.Should().Be("New H");
        template.TextBody.Should().Be("New T");
        template.Version.Should().Be(2);
    }

    [Fact]
    public void UpdateContent_WhenSystemTemplate_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var template = EmailTemplate.Create("test", "S", "H", "T", isSystemTemplate: true);

        // Act
        var act = () => template.UpdateContent("New S", "New H", "New T");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Deactivate_WhenNotSystemTemplate_ShouldDeactivate()
    {
        // Arrange
        var template = EmailTemplate.Create("test", "S", "H", "T", isSystemTemplate: false);

        // Act
        template.Deactivate();

        // Assert
        template.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenSystemTemplate_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var template = EmailTemplate.Create("test", "S", "H", "T", isSystemTemplate: true);

        // Act
        var act = () => template.Deactivate();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Activate_ShouldActivate()
    {
        // Arrange
        var template = EmailTemplate.Create("test", "S", "H", "T", isSystemTemplate: false);
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
        var act = () => EmailTemplate.Create(key!, sub!, html!, text!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null, "H", "T")]
    [InlineData("S", null, "T")]
    [InlineData("S", "H", null)]
    public void UpdateContent_WithInvalidData_ShouldThrowArgumentException(string? sub, string? html, string? text)
    {
        // Arrange
        var template = EmailTemplate.Create("test", "S", "H", "T", isSystemTemplate: false);

        // Act
        var act = () => template.UpdateContent(sub!, html!, text!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
