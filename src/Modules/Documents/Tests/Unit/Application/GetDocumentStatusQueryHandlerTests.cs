using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application;

public class GetDocumentStatusQueryHandlerTests
{
    private readonly Mock<IDocumentQueries> _mockQueries;
    private readonly Mock<ILogger<GetDocumentStatusQueryHandler>> _mockLogger;
    private readonly GetDocumentStatusQueryHandler _handler;

    public GetDocumentStatusQueryHandlerTests()
    {
        _mockQueries = new Mock<IDocumentQueries>();
        _mockLogger = new Mock<ILogger<GetDocumentStatusQueryHandler>>();
        _handler = new GetDocumentStatusQueryHandler(_mockQueries.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingDocument_ShouldReturnDocumentDto()
    {
        var document = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "test.pdf", "blob-url");
        var documentId = document.Id;

        _mockQueries.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var query = new GetDocumentStatusQuery(documentId);
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(document.Id);
    }
}
