using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Modules.Communications.Application.Handlers.Commands;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Communications;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers.Commands;

public class EmailTemplateCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<EmailTemplate, Guid>> _repositoryMock;
    private readonly EmailTemplateCommandHandler _handler;

    public EmailTemplateCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<EmailTemplate, Guid>>();
        _uowMock.Setup(x => x.GetRepository<EmailTemplate, Guid>()).Returns(_repositoryMock.Object);
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _handler = new EmailTemplateCommandHandler(_uowMock.Object);
    }

    [Fact]
    public async Task HandleAsync_CreateTemplate_ShouldAddAndSave()
    {
        // Arrange
        var command = new CreateEmailTemplateCommand("welcome", "Sub", "Html", "Text", true, "pt-BR", Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(x => x.Add(It.IsAny<EmailTemplate>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UpdateTemplate_WhenFound_ShouldUpdateAndSave()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = new EmailTemplateBuilder()
            .WithKey("welcome")
            .WithSubject("OldSub")
            .WithHtmlBody("OldHtml")
            .WithTextBody("OldText")
            .Build();
        _repositoryMock.Setup(x => x.TryFindAsync(templateId, It.IsAny<CancellationToken>())).ReturnsAsync(template);

        var command = new UpdateEmailTemplateCommand(templateId, "NewSub", "NewHtml", "NewText", Guid.NewGuid());

        EmailTemplate? capturedNewVersion = null;
        _repositoryMock.Setup(x => x.Add(It.IsAny<EmailTemplate>()))
            .Callback<EmailTemplate>(t => capturedNewVersion = t);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        template.IsActive.Should().BeFalse("old version should be deactivated");
        capturedNewVersion.Should().NotBeNull("a new version should have been created");
        capturedNewVersion!.Subject.Should().Be("NewSub");
        capturedNewVersion.HtmlBody.Should().Be("NewHtml");
        capturedNewVersion.TextBody.Should().Be("NewText");
        capturedNewVersion.Version.Should().Be(2);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UpdateTemplate_WhenNotFound_ShouldReturnFailure()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.TryFindAsync(templateId, It.IsAny<CancellationToken>())).ReturnsAsync((EmailTemplate?)null);

        var command = new UpdateEmailTemplateCommand(templateId, "NewSub", "NewHtml", "NewText", Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_UpdateTemplate_WhenSystemTemplate_ShouldReturnBadRequest()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = new EmailTemplateBuilder()
            .WithKey("welcome")
            .WithSubject("OldSub")
            .WithHtmlBody("OldHtml")
            .WithTextBody("OldText")
            .WithLanguage("pt-br")
            .AsSystemTemplate()
            .Build();
        _repositoryMock.Setup(x => x.TryFindAsync(templateId, It.IsAny<CancellationToken>())).ReturnsAsync(template);

        var command = new UpdateEmailTemplateCommand(templateId, "NewSub", "NewHtml", "NewText", Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task HandleAsync_UpdateTemplate_ShouldPreserveProperties()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = new EmailTemplateBuilder()
            .WithKey("welcome")
            .WithSubject("OldSub")
            .WithHtmlBody("OldHtml")
            .WithTextBody("OldText")
            .WithLanguage("en-US")
            .WithOverrideKey("custom-override")
            .Build();
        _repositoryMock.Setup(x => x.TryFindAsync(templateId, It.IsAny<CancellationToken>())).ReturnsAsync(template);

        var command = new UpdateEmailTemplateCommand(templateId, "NewSub", "NewHtml", "NewText", Guid.NewGuid());

        EmailTemplate? capturedNewVersion = null;
        _repositoryMock.Setup(x => x.Add(It.IsAny<EmailTemplate>()))
            .Callback<EmailTemplate>(t => capturedNewVersion = t);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedNewVersion!.TemplateKey.Should().Be("welcome");
        capturedNewVersion.Language.Should().Be("en-us");
        capturedNewVersion.OverrideKey.Should().Be("custom-override");
    }

    [Fact]
    public async Task HandleAsync_SetStatus_WhenNotFound_ShouldReturnFailure()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        _repositoryMock.Setup(x => x.TryFindAsync(templateId, It.IsAny<CancellationToken>())).ReturnsAsync((EmailTemplate?)null);

        var command = new SetEmailTemplateStatusCommand(templateId, true, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_SetStatus_Activate_WhenFound_ShouldActivateAndSave()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = new EmailTemplateBuilder()
            .WithKey("key")
            .WithSubject("Sub")
            .WithHtmlBody("Html")
            .WithTextBody("Text")
            .Build();
        template.Deactivate();
        _repositoryMock.Setup(x => x.TryFindAsync(templateId, It.IsAny<CancellationToken>())).ReturnsAsync(template);

        var command = new SetEmailTemplateStatusCommand(templateId, true, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        template.IsActive.Should().BeTrue();
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SetStatus_Deactivate_WhenFound_ShouldDeactivateAndSave()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = new EmailTemplateBuilder()
            .WithKey("key")
            .WithSubject("Sub")
            .WithHtmlBody("Html")
            .WithTextBody("Text")
            .Build();
        _repositoryMock.Setup(x => x.TryFindAsync(templateId, It.IsAny<CancellationToken>())).ReturnsAsync(template);

        var command = new SetEmailTemplateStatusCommand(templateId, false, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        template.IsActive.Should().BeFalse();
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SetStatus_DeactivateSystemTemplate_ShouldReturnBadRequest()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = new EmailTemplateBuilder()
            .WithKey("key")
            .WithSubject("Sub")
            .WithHtmlBody("Html")
            .WithTextBody("Text")
            .WithLanguage("pt-br")
            .AsSystemTemplate()
            .Build();
        _repositoryMock.Setup(x => x.TryFindAsync(templateId, It.IsAny<CancellationToken>())).ReturnsAsync(template);

        var command = new SetEmailTemplateStatusCommand(templateId, false, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.StatusCode.Should().Be(400);
    }
}
