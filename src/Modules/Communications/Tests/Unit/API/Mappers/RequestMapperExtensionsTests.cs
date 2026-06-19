using MeAjudaAi.Modules.Communications.API.Mappers;
using MeAjudaAi.Modules.Communications.Application.DTOs;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.API.Mappers;

[Trait("Category", "Unit")]
[Trait("Module", "Communications")]
[Trait("Layer", "API")]
public class RequestMapperExtensionsTests
{
    [Fact]
    public void ToCommand_UpdateEmailTemplateBody_ShouldMapAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var body = new UpdateEmailTemplateBody(
            Subject: "Welcome Email",
            HtmlBody: "<h1>Welcome</h1>",
            TextBody: "Welcome");

        // Act
        var command = body.ToCommand(id, correlationId);

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(id);
        command.Subject.Should().Be("Welcome Email");
        command.HtmlBody.Should().Be("<h1>Welcome</h1>");
        command.TextBody.Should().Be("Welcome");
        command.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void ToActivateCommand_ShouldMapIdAndIsActiveTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        // Act
        var command = id.ToActivateCommand(correlationId);

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(id);
        command.IsActive.Should().BeTrue();
        command.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public void ToDeactivateCommand_ShouldMapIdAndIsActiveFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        // Act
        var command = id.ToDeactivateCommand(correlationId);

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(id);
        command.IsActive.Should().BeFalse();
        command.CorrelationId.Should().Be(correlationId);
    }
}
