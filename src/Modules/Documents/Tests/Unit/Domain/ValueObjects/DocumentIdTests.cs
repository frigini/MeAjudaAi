using FluentAssertions;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.ValueObjects;

public sealed class DocumentIdTests
{
    [Fact]
    public void Constructor_WithValidGuid_ShouldCreateDocumentId()
    {
        // Preparação
        var guid = Guid.NewGuid();

        // Ação
        var documentId = new DocumentId(guid);

        // Verificação
        documentId.Value.Should().Be(guid);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Preparação
        var emptyGuid = Guid.Empty;

        // Ação
        var act = () => new DocumentId(emptyGuid);

        // Verificação
        act.Should().Throw<ArgumentException>()
            .WithMessage("DocumentId cannot be empty*");
    }

    [Fact]
    public void New_ShouldGenerateValidDocumentId()
    {
        // Ação
        var documentId = DocumentId.New();

        // Verificação
        documentId.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Preparação
        var guid = Guid.NewGuid();
        var documentId1 = new DocumentId(guid);
        var documentId2 = new DocumentId(guid);

        // Ação & Assert
        documentId1.Should().Be(documentId2);
        (documentId1 == documentId2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Preparação
        var documentId1 = DocumentId.New();
        var documentId2 = DocumentId.New();

        // Ação & Assert
        documentId1.Should().NotBe(documentId2);
        (documentId1 != documentId2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Preparação
        var guid = Guid.NewGuid();
        var documentId1 = new DocumentId(guid);
        var documentId2 = new DocumentId(guid);

        // Ação & Assert
        documentId1.GetHashCode().Should().Be(documentId2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnGuidAsString()
    {
        // Preparação
        var guid = Guid.NewGuid();
        var documentId = new DocumentId(guid);

        // Ação
        var result = documentId.ToString();

        // Verificação
        result.Should().Contain(guid.ToString());
    }
}
