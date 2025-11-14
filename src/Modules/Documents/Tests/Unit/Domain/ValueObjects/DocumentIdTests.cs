using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Domain.ValueObjects;

public class DocumentIdTests
{
    [Fact]
    public void Constructor_WithValidGuid_ShouldCreateDocumentId()
    {
        // Arrange
        var guid = UuidGenerator.NewId();

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

        // Act & Assert
        var act = () => new DocumentId(emptyGuid);
        act.Should().Throw<ArgumentException>()
            .WithMessage("DocumentId cannot be empty*")
            .And.ParamName.Should().Be("value");
    }

    [Fact]
    public void New_ShouldCreateDocumentIdWithUniqueGuid()
    {
        // Act
        var documentId1 = DocumentId.New();
        var documentId2 = DocumentId.New();

        // Assert
        documentId1.Value.Should().NotBe(Guid.Empty);
        documentId2.Value.Should().NotBe(Guid.Empty);
        documentId1.Value.Should().NotBe(documentId2.Value);
    }

    [Fact]
    public void ImplicitOperator_ToGuid_ShouldReturnGuidValue()
    {
        // Arrange
        var guid = UuidGenerator.NewId();
        var documentId = new DocumentId(guid);

        // Act
        Guid result = documentId;

        // Assert
        result.Should().Be(guid);
    }

    [Fact]
    public void ImplicitOperator_FromGuid_ShouldCreateDocumentId()
    {
        // Arrange
        var guid = UuidGenerator.NewId();

        // Act
        DocumentId documentId = guid;

        // Assert
        documentId.Value.Should().Be(guid);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var guid = UuidGenerator.NewId();
        var documentId1 = new DocumentId(guid);
        var documentId2 = new DocumentId(guid);

        // Act & Assert
        documentId1.Should().Be(documentId2);
        documentId1.GetHashCode().Should().Be(documentId2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var documentId1 = DocumentId.New();
        var documentId2 = DocumentId.New();

        // Act & Assert
        documentId1.Should().NotBe(documentId2);
        // Note: While hash contract only requires equal objects to have equal hashes,
        // GUID hash collisions are astronomically unlikely in practice
        documentId1.GetHashCode().Should().NotBe(documentId2.GetHashCode());
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var documentId = DocumentId.New();

        // Act & Assert
        documentId.Should().NotBeNull();
        documentId.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void ImplicitOperator_ToGuid_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        DocumentId? documentId = null;

        // Act & Assert
        var act = () =>
        {
            Guid result = documentId!;
            return result;
        };
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = UuidGenerator.NewId();
        var documentId = new DocumentId(guid);

        // Act
        var result = documentId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }
}
