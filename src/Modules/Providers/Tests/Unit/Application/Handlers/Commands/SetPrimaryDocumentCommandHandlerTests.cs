using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class SetPrimaryDocumentCommandHandlerTests
{
    private readonly Mock<IProviderUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<ILogger<SetPrimaryDocumentCommandHandler>> _loggerMock;
    private readonly SetPrimaryDocumentCommandHandler _handler;

    public SetPrimaryDocumentCommandHandlerTests()
    {
        _uowMock = new Mock<IProviderUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _loggerMock = new Mock<ILogger<SetPrimaryDocumentCommandHandler>>();

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        _handler = new SetPrimaryDocumentCommandHandler(_uowMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder().WithId(providerId).Build();
        provider.AddDocument(new Document("12345678901", EDocumentType.CPF, "cpf.pdf", "url", false));

        var command = new SetPrimaryDocumentCommand(providerId, EDocumentType.CPF);

        _providerRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.GetPrimaryDocument()?.DocumentType.Should().Be(EDocumentType.CPF);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentProvider_ReturnsNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new SetPrimaryDocumentCommand(providerId, EDocumentType.CPF);

        _providerRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDocument_ReturnsNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder().WithId(providerId).Build();
        // Não adicionamos documentos

        var command = new SetPrimaryDocumentCommand(providerId, EDocumentType.CPF);

        _providerRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithDomainException_ReturnsFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder().WithId(providerId).Build();
        provider.AddDocument(new Document("12345678901", EDocumentType.CPF, "cpf.pdf", "url", false));
        provider.Delete(TimeProvider.System); 

        var command = new SetPrimaryDocumentCommand(providerId, EDocumentType.CPF);

        _providerRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithRepositoryException_ReturnsInternalError()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new SetPrimaryDocumentCommand(providerId, EDocumentType.CPF);

        _providerRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Exception("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(500);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}


