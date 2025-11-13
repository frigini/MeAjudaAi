using MeAjudaAi.Modules.Documents.Domain.Enums;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Domain.Enums;

public class DocumentStatusTests
{
    [Theory]
    [InlineData(DocumentStatus.Uploaded, 1)]
    [InlineData(DocumentStatus.PendingVerification, 2)]
    [InlineData(DocumentStatus.Verified, 3)]
    [InlineData(DocumentStatus.Rejected, 4)]
    [InlineData(DocumentStatus.Failed, 5)]
    public void DocumentStatus_ShouldHaveCorrectValues(DocumentStatus status, int expectedValue)
    {
        // Assert
        ((int)status).Should().Be(expectedValue);
    }

    [Fact]
    public void DocumentStatus_ShouldHaveAllExpectedValues()
    {
        // Arrange
        var expectedStatuses = new[]
        {
            DocumentStatus.Uploaded,
            DocumentStatus.PendingVerification,
            DocumentStatus.Verified,
            DocumentStatus.Rejected,
            DocumentStatus.Failed
        };

        // Act
        var allStatuses = Enum.GetValues<DocumentStatus>();

        // Assert
        allStatuses.Should().BeEquivalentTo(expectedStatuses);
    }
}
