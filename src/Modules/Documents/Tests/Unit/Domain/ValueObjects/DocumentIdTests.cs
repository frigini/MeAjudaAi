using FluentAssertions;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.ValueObjects;

public sealed class DocumentIdTests
{
    [Fact]
    public void Constructor_WithValidGuid_ShouldCreateDocumentId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var documentId = new DocumentId(guid);

        // Assert
        documentId.Value.Should().Be(guid);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var act = () => new DocumentId(emptyGuid);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("DocumentId cannot be empty*");
    }

    [Fact]
    public void New_ShouldGenerateValidDocumentId()
    {
        // Act
        var documentId = DocumentId.New();

        // Assert
        documentId.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var documentId1 = new DocumentId(guid);
        var documentId2 = new DocumentId(guid);

        // Act & Assert
        documentId1.Should().Be(documentId2);
        (documentId1 == documentId2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var documentId1 = DocumentId.New();
        var documentId2 = DocumentId.New();

        // Act & Assert
        documentId1.Should().NotBe(documentId2);
        (documentId1 != documentId2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var documentId1 = new DocumentId(guid);
        var documentId2 = new DocumentId(guid);

        // Act & Assert
        documentId1.GetHashCode().Should().Be(documentId2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnGuidAsString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var documentId = new DocumentId(guid);

        // Act
        var result = documentId.ToString();

        // Assert
        result.Should().Contain(guid.ToString());
    }
}
