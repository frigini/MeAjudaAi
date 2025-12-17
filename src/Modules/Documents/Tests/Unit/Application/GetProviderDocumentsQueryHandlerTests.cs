using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application;

public class GetProviderDocumentsQueryHandlerTests
{
    private readonly Mock<IDocumentRepository> _mockRepository;
    private readonly GetProviderDocumentsQueryHandler _handler;

    public GetProviderDocumentsQueryHandlerTests()
    {
        _mockRepository = new Mock<IDocumentRepository>();
        _handler = new GetProviderDocumentsQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingDocuments_ShouldReturnDocumentList()
    {
        // Preparação
        var providerId = Guid.NewGuid();
        var documents = new List<Document>
        {
            Document.Create(providerId, EDocumentType.IdentityDocument, "doc1.pdf", "url1"),
            Document.Create(providerId, EDocumentType.ProofOfResidence, "doc2.pdf", "url2")
        };

        _mockRepository.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        var query = new GetProviderDocumentsQuery(providerId);

        // Ação
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Verificação
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Select(d => d.DocumentType).Should().Contain(new[] { EDocumentType.IdentityDocument, EDocumentType.ProofOfResidence });
        _mockRepository.Verify(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNoDocuments_ShouldReturnEmptyList()
    {
        // Preparação
        var providerId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document>());

        var query = new GetProviderDocumentsQuery(providerId);

        // Ação
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Verificação
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldMapAllDocumentProperties()
    {
        // Preparação
        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");

        // Transicionar documento para testar propriedades opcionais com valores não-nulos
        document.MarkAsPendingVerification();
        document.MarkAsVerified("{\"cpf\":\"12345678900\"}");
        // Não podemos marcar como rejeitado após verificado - testar apenas verified state

        _mockRepository.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document> { document });

        var query = new GetProviderDocumentsQuery(providerId);

        // Ação
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Verificação
        var dto = result.Single();
        dto.Id.Should().Be(document.Id);
        dto.ProviderId.Should().Be(document.ProviderId);
        dto.DocumentType.Should().Be(document.DocumentType);
        dto.FileName.Should().Be(document.FileName);
        dto.FileUrl.Should().Be(document.FileUrl);
        dto.Status.Should().Be(document.Status);
        dto.UploadedAt.Should().Be(document.UploadedAt);
        dto.VerifiedAt.Should().NotBeNull();
        dto.VerifiedAt.Should().Be(document.VerifiedAt);
        dto.RejectionReason.Should().BeNull(); // Verified, não rejected
        dto.OcrData.Should().NotBeNullOrEmpty();
        dto.OcrData.Should().Be(document.OcrData);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Preparação
        var providerId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var query = new GetProviderDocumentsQuery(providerId);

        // Ação & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.HandleAsync(query, CancellationToken.None));
    }
}
