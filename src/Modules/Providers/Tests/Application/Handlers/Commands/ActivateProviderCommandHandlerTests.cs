using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Contracts.Modules.Documents;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Application.Handlers.Commands;

public class ActivateProviderCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<IDocumentsModuleApi> _documentsModuleApiMock;
    private readonly Mock<ILogger<ActivateProviderCommandHandler>> _loggerMock;
    private readonly ActivateProviderCommandHandler _handler;

    public ActivateProviderCommandHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _documentsModuleApiMock = new Mock<IDocumentsModuleApi>();
        _loggerMock = new Mock<ILogger<ActivateProviderCommandHandler>>();
        _handler = new ActivateProviderCommandHandler(_providerRepositoryMock.Object, _documentsModuleApiMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new ActivateProviderCommand(Guid.NewGuid(), "admin@test.com");

        _providerRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ValidationMessages.Providers.NotFound);
    }

    [Fact]
    public async Task HandleAsync_WhenRequiredDocumentsMissing_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new ActivateProviderCommand(providerId, "admin@test.com");
        var provider = CreateProvider(providerId);

        _providerRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _documentsModuleApiMock.Setup(x => x.HasRequiredDocumentsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ValidationMessages.Providers.MustHaveAllDocuments);
    }

    [Fact]
    public async Task HandleAsync_WhenDocumentsNotVerified_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new ActivateProviderCommand(providerId, "admin@test.com");
        var provider = CreateProvider(providerId);

        _providerRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _documentsModuleApiMock.Setup(x => x.HasRequiredDocumentsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));
        
        _documentsModuleApiMock.Setup(x => x.HasVerifiedDocumentsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ValidationMessages.Providers.MustHaveVerifiedDocuments);
    }

    [Fact]
    public async Task HandleAsync_WhenDocumentsPending_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new ActivateProviderCommand(providerId, "admin@test.com");
        var provider = CreateProvider(providerId);

        _providerRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _documentsModuleApiMock.Setup(x => x.HasRequiredDocumentsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));
        
        _documentsModuleApiMock.Setup(x => x.HasVerifiedDocumentsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _documentsModuleApiMock.Setup(x => x.HasPendingDocumentsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ValidationMessages.Providers.CannotBeActivatedPendingDocs);
    }

    [Fact]
    public async Task HandleAsync_WhenDocumentsRejected_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new ActivateProviderCommand(providerId, "admin@test.com");
        var provider = CreateProvider(providerId);

        _providerRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _documentsModuleApiMock.Setup(x => x.HasRequiredDocumentsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));
        
        _documentsModuleApiMock.Setup(x => x.HasVerifiedDocumentsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _documentsModuleApiMock.Setup(x => x.HasPendingDocumentsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));
            
        _documentsModuleApiMock.Setup(x => x.HasRejectedDocumentsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ValidationMessages.Providers.CannotBeActivatedRejectedDocs);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldActivateAndReturnSuccess()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new ActivateProviderCommand(providerId, "admin@test.com");
        var provider = CreateProvider(providerId);
        
        // Configura o status do provider para ser elegível para ativação (PendingDocumentVerification)
        provider.CompleteBasicInfo("admin@test.com");

        _providerRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _documentsModuleApiMock.Setup(x => x.HasRequiredDocumentsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));
        
        _documentsModuleApiMock.Setup(x => x.HasVerifiedDocumentsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        _documentsModuleApiMock.Setup(x => x.HasPendingDocumentsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));
            
        _documentsModuleApiMock.Setup(x => x.HasRejectedDocumentsAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Status.Should().Be(EProviderStatus.Active);
        provider.VerificationStatus.Should().Be(EVerificationStatus.Verified);
        
        _providerRepositoryMock.Verify(r => r.UpdateAsync(provider, It.IsAny<CancellationToken>()), Times.Once);
    }

    private Provider CreateProvider(Guid id)
    {
         return new Provider(
             new ProviderId(id),
             Guid.NewGuid(),
             "Name",
             EProviderType.Individual,
             new BusinessProfile("Legal", new ContactInfo("email@test.com"), new Address("Street", "1", "Neighborhood", "City", "ST", "12345678", "Country"))
         );
    }
}
