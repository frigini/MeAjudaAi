using MeAjudaAi.Modules.Documents.Domain.Enums;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Domain.Enums;

public class DocumentTypeTests
{
    [Theory]
    [InlineData(DocumentType.IdentityDocument, 1)]
    [InlineData(DocumentType.ProofOfResidence, 2)]
    [InlineData(DocumentType.CriminalRecord, 3)]
    [InlineData(DocumentType.Other, 99)]
    public void DocumentType_ShouldHaveCorrectValues(DocumentType type, int expectedValue)
    {
        // Assert
        ((int)type).Should().Be(expectedValue);
    }

    [Fact]
    public void DocumentType_ShouldHaveAllExpectedValues()
    {
        // Arrange
        var expectedTypes = new[]
        {
            DocumentType.IdentityDocument,
            DocumentType.ProofOfResidence,
            DocumentType.CriminalRecord,
            DocumentType.Other
        };

        // Act
        var allTypes = Enum.GetValues<DocumentType>();

        // Assert
        allTypes.Should().BeEquivalentTo(expectedTypes);
    }
}
