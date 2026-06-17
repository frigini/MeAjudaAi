using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application.Handlers.Queries;

public class GetProviderDocumentsQueryHandlerTests
{
    private readonly Mock<IDocumentQueries> _mockQueries;
    private readonly GetProviderDocumentsQueryHandler _handler;

    public GetProviderDocumentsQueryHandlerTests()
    {
        _mockQueries = new Mock<IDocumentQueries>();
        _handler = new GetProviderDocumentsQueryHandler(_mockQueries.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingDocuments_ShouldReturnDocumentList()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documents = new List<Document>
        {
            Document.Create(providerId, EDocumentType.IdentityDocument, "doc1.pdf", "url1"),
            Document.Create(providerId, EDocumentType.ProofOfResidence, "doc2.pdf", "url2")
        };

        _mockQueries.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        var query = new GetProviderDocumentsQuery(providerId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.Id == documents[0].Id);
        result.Should().Contain(d => d.Id == documents[1].Id);

        _mockQueries.Verify(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
