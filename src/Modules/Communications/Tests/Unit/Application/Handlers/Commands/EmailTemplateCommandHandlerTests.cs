using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Modules.Communications.Application.Handlers.Commands;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Shared.Database.Abstractions;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Handlers.Commands;

public class EmailTemplateCommandHandlerTests
{
    private readonly Mock<IRepository<EmailTemplate, Guid>> _templateRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly EmailTemplateCommandHandler _handler;

    public EmailTemplateCommandHandlerTests()
    {
        _templateRepositoryMock = new Mock<IRepository<EmailTemplate, Guid>>();
        _uowMock = new Mock<IUnitOfWork>();
        _handler = new EmailTemplateCommandHandler(_templateRepositoryMock.Object, _uowMock.Object);
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
        _templateRepositoryMock.Verify(x => x.Add(It.IsAny<EmailTemplate>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UpdateTemplate_WhenFound_ShouldUpdateAndSave()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = EmailTemplate.Create("welcome", "OldSub", "OldHtml", "OldText");
        _templateRepositoryMock.Setup(x => x.TryFindAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
            
        var command = new UpdateEmailTemplateCommand(templateId, "NewSub", "NewHtml", "NewText", Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        template.Subject.Should().Be("NewSub");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UpdateTemplate_WhenNotFound_ShouldReturnFailure()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        _templateRepositoryMock.Setup(x => x.TryFindAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailTemplate?)null);
            
        var command = new UpdateEmailTemplateCommand(templateId, "NewSub", "NewHtml", "NewText", Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_SetStatus_WhenFound_ShouldUpdateStatusAndSave()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = EmailTemplate.Create("welcome", "Sub", "Html", "Text");
        _templateRepositoryMock.Setup(x => x.TryFindAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
            
        var command = new SetEmailTemplateStatusCommand(templateId, false, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        template.IsActive.Should().BeFalse();
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
