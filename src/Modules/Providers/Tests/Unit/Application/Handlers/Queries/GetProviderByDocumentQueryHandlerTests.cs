using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Application.Handlers.Queries;

[Trait("Category", "Unit")]
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
    public async Task HandleAsync_WithValidDocument_ShouldReturnProviderDto()
    {
        // Arrange
        var document = "12345678901";
        var provider = new ProviderBuilder()
            .WithDocument(document, EDocumentType.CPF)
            .Build();

        _providerRepositoryMock
            .Setup(x => x.GetByDocumentAsync(document, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetProviderByDocumentQuery(document);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(provider.Id.Value);

        _providerRepositoryMock.Verify(
            x => x.GetByDocumentAsync(document, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithDocumentNotFound_ShouldReturnSuccessWithNull()
    {
        // Arrange
        var document = "99999999999";

        _providerRepositoryMock
            .Setup(x => x.GetByDocumentAsync(document, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        var query = new GetProviderByDocumentQuery(document);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();

        _providerRepositoryMock.Verify(
            x => x.GetByDocumentAsync(document, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithWhitespaceDocument_ShouldReturnBadRequest()
    {
        // Arrange
        var query = new GetProviderByDocumentQuery("   ");

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(400);
        result.Error.Message.Should().Contain("cannot be empty");

        _providerRepositoryMock.Verify(
            x => x.GetByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyDocument_ShouldReturnBadRequest()
    {
        // Arrange
        var query = new GetProviderByDocumentQuery(string.Empty);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(400);
        result.Error.Message.Should().Contain("cannot be empty");

        _providerRepositoryMock.Verify(
            x => x.GetByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNullDocument_ShouldReturnBadRequest()
    {
        // Arrange
        var query = new GetProviderByDocumentQuery(null!);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(400);
        result.Error.Message.Should().Contain("cannot be empty");

        _providerRepositoryMock.Verify(
            x => x.GetByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithDocumentHavingWhitespace_ShouldTrimAndSearch()
    {
        // Arrange
        var document = "  12345678901  ";
        var trimmedDocument = "12345678901";
        var provider = new ProviderBuilder()
            .WithDocument(trimmedDocument, EDocumentType.CPF)
            .Build();

        _providerRepositoryMock
            .Setup(x => x.GetByDocumentAsync(trimmedDocument, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetProviderByDocumentQuery(document);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        _providerRepositoryMock.Verify(
            x => x.GetByDocumentAsync(trimmedDocument, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnInternalError()
    {
        // Arrange
        var document = "12345678901";
        var exception = new Exception("Database connection failed");

        _providerRepositoryMock
            .Setup(x => x.GetByDocumentAsync(document, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var query = new GetProviderByDocumentQuery(document);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(500);
        result.Error.Message.Should().Contain("error occurred");

        _providerRepositoryMock.Verify(
            x => x.GetByDocumentAsync(document, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var document = "12345678901";
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _providerRepositoryMock
            .Setup(x => x.GetByDocumentAsync(document, cancellationToken))
            .ReturnsAsync((Provider?)null);

        var query = new GetProviderByDocumentQuery(document);

        // Act
        await _handler.HandleAsync(query, cancellationToken);

        // Assert
        _providerRepositoryMock.Verify(
            x => x.GetByDocumentAsync(document, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCompleteProvider_ShouldMapAllProperties()
    {
        // Arrange
        var document = "12345678901";
        var provider = new ProviderBuilder()
            .WithDocument(document, EDocumentType.CPF)
            .WithQualification("Engineering Degree", "Description", "UnivX", DateTime.Now.AddYears(-2), DateTime.Now.AddYears(5), "12345678901")
            .Build();

        _providerRepositoryMock
            .Setup(x => x.GetByDocumentAsync(document, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetProviderByDocumentQuery(document);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.BusinessProfile.Should().NotBeNull();
        result.Value.Documents.Should().HaveCount(1);
        result.Value.Qualifications.Should().HaveCount(1);
    }
}
