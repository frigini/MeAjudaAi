using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
public class GetProviderByDocumentQueryHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<GetProviderByDocumentQueryHandler>> _loggerMock;
    private readonly GetProviderByDocumentQueryHandler _handler;

    public GetProviderByDocumentQueryHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<GetProviderByDocumentQueryHandler>>();
        _handler = new GetProviderByDocumentQueryHandler(_providerRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidDocument_ShouldReturnProviderSuccessfully()
    {
        // Arrange
        var document = "12345678901";
        var query = new GetProviderByDocumentQuery(document);
        var provider = CreateValidProvider(document);

        _providerRepositoryMock
            .Setup(r => r.GetByDocumentAsync(document, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(provider.Id.Value);
        result.Value.Name.Should().Be(provider.Name);
        result.Value.Type.Should().Be(provider.Type);
        result.Value.VerificationStatus.Should().Be(provider.VerificationStatus);
        result.Value.BusinessProfile.Should().NotBeNull();

        _providerRepositoryMock.Verify(
            r => r.GetByDocumentAsync(document, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithDocumentWithWhitespace_ShouldTrimAndSearch()
    {
        // Arrange
        var documentWithSpaces = "  12345678901  ";
        var trimmedDocument = "12345678901";
        var query = new GetProviderByDocumentQuery(documentWithSpaces);
        var provider = CreateValidProvider(trimmedDocument);

        _providerRepositoryMock
            .Setup(r => r.GetByDocumentAsync(trimmedDocument, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        _providerRepositoryMock.Verify(
            r => r.GetByDocumentAsync(trimmedDocument, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyDocument_ShouldReturnBadRequestError()
    {
        // Arrange
        var query = new GetProviderByDocumentQuery("");

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(400);
        result.Error.Message.Should().Be("Document cannot be empty");

        _providerRepositoryMock.Verify(
            r => r.GetByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        // Assert: Warning should be logged for invalid document
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid document provided")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithWhitespaceOnlyDocument_ShouldReturnBadRequestError()
    {
        // Arrange
        var query = new GetProviderByDocumentQuery("   ");

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(400);
        result.Error.Message.Should().Be("Document cannot be empty");

        _providerRepositoryMock.Verify(
            r => r.GetByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNullDocument_ShouldReturnBadRequestError()
    {
        // Arrange
        var query = new GetProviderByDocumentQuery(null!);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(400);
        result.Error.Message.Should().Be("Document cannot be empty");

        _providerRepositoryMock.Verify(
            r => r.GetByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnNullSuccessfully()
    {
        // Arrange
        var document = "99999999999";
        var query = new GetProviderByDocumentQuery(document);

        _providerRepositoryMock
            .Setup(r => r.GetByDocumentAsync(document, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();

        _providerRepositoryMock.Verify(
            r => r.GetByDocumentAsync(document, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnInternalError()
    {
        // Arrange
        var document = "12345678901";
        var query = new GetProviderByDocumentQuery(document);
        var exception = new InvalidOperationException("Database connection failed");

        _providerRepositoryMock
            .Setup(r => r.GetByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(500);
        result.Error.Message.Should().Contain("An error occurred while searching for the provider");

        _providerRepositoryMock.Verify(
            r => r.GetByDocumentAsync(document, It.IsAny<CancellationToken>()),
            Times.Once);

        // Assert: Error should be logged with exception details
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while searching for provider by document")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldLogInformationMessages()
    {
        // Arrange
        var document = "12345678901";
        var query = new GetProviderByDocumentQuery(document);
        var provider = CreateValidProvider(document);

        _providerRepositoryMock
            .Setup(r => r.GetByDocumentAsync(document, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Searching for provider by document")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Provider found for document")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static Provider CreateValidProvider(string document)
    {
        var providerId = new ProviderId(Guid.CreateVersion7());
        var userId = Guid.CreateVersion7();
        var address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01234-567", "Brasil");
        var contactInfo = new ContactInfo("test@test.com", "11999999999");
        var businessProfile = new BusinessProfile("Test Provider LTDA", contactInfo, address, document);

        return new Provider(providerId, userId, "Test Provider", EProviderType.Individual, businessProfile);
    }
}
