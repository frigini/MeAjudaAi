using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Application.Handlers.Queries;

[Trait("Category", "Unit")]
public class GetProviderByDocumentQueryHandlerTests
{
    private readonly Mock<IProviderQueries> _providerQueriesMock;
    private readonly Mock<ILogger<GetProviderByDocumentQueryHandler>> _loggerMock;
    private readonly Mock<IStringLocalizer<Strings>> _localizerMock;
    private readonly GetProviderByDocumentQueryHandler _handler;

    public GetProviderByDocumentQueryHandlerTests()
    {
        _providerQueriesMock = new Mock<IProviderQueries>();
        _loggerMock = new Mock<ILogger<GetProviderByDocumentQueryHandler>>();
        _localizerMock = new Mock<IStringLocalizer<Strings>>();
        _localizerMock.Setup(x => x[It.Is<string>(s => s == "DocumentRequired")]).Returns(new LocalizedString("DocumentRequired", "O documento não pode ser vazio."));
        _handler = new GetProviderByDocumentQueryHandler(_providerQueriesMock.Object, _loggerMock.Object, _localizerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidDocument_ShouldReturnProviderDto()
    {
        // Arrange
        var document = "12345678901";
        var provider = new ProviderBuilder()
            .WithDocument(document, EDocumentType.CPF)
            .Build();

        _providerQueriesMock
            .Setup(x => x.GetByDocumentAsync(document, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetProviderByDocumentQuery(document);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(provider.Id.Value);

        _providerQueriesMock.Verify(
            x => x.GetByDocumentAsync(document, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithDocumentNotFound_ShouldReturnSuccessWithNull()
    {
        // Arrange
        var document = "99999999999";

        _providerQueriesMock
            .Setup(x => x.GetByDocumentAsync(document, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        var query = new GetProviderByDocumentQuery(document);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();

        _providerQueriesMock.Verify(
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
        result.Error.Message.Should().Contain("não pode ser vazio");

        _providerQueriesMock.Verify(
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
        result.Error.Message.Should().Contain("não pode ser vazio");

        _providerQueriesMock.Verify(
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
        result.Error.Message.Should().Contain("não pode ser vazio");

        _providerQueriesMock.Verify(
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

        _providerQueriesMock
            .Setup(x => x.GetByDocumentAsync(trimmedDocument, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetProviderByDocumentQuery(document);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        _providerQueriesMock.Verify(
            x => x.GetByDocumentAsync(trimmedDocument, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldThrow()
    {
        // Arrange
        var document = "12345678901";
        var exception = new Exception("Database connection failed");

        _providerQueriesMock
            .Setup(x => x.GetByDocumentAsync(document, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var query = new GetProviderByDocumentQuery(document);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(query));
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var document = "12345678901";
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _providerQueriesMock
            .Setup(x => x.GetByDocumentAsync(document, cancellationToken))
            .ReturnsAsync((Provider?)null);

        var query = new GetProviderByDocumentQuery(document);

        // Act
        await _handler.HandleAsync(query, cancellationToken);

        // Assert
        _providerQueriesMock.Verify(
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

        _providerQueriesMock
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
