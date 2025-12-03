using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Enums;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Enums;

public sealed class EProviderStatusTests
{
    [Fact]
    public void ProviderStatus_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)EProviderStatus.None).Should().Be(0);
        ((int)EProviderStatus.PendingBasicInfo).Should().Be(1);
        ((int)EProviderStatus.PendingDocumentVerification).Should().Be(2);
        ((int)EProviderStatus.Active).Should().Be(3);
        ((int)EProviderStatus.Suspended).Should().Be(4);
    }

    [Fact]
    public void ProviderStatus_ShouldHaveAllExpectedMembers()
    {
        // Arrange
        var expectedStatuses = new[]
        {
            EProviderStatus.None,
            EProviderStatus.PendingBasicInfo,
            EProviderStatus.PendingDocumentVerification,
            EProviderStatus.Active,
            EProviderStatus.Suspended
        };

        // Act
        var actualStatuses = Enum.GetValues<EProviderStatus>();

        // Assert
        actualStatuses.Should().BeEquivalentTo(expectedStatuses);
        actualStatuses.Should().HaveCount(5);
    }

    [Theory]
    [InlineData(EProviderStatus.None, "None")]
    [InlineData(EProviderStatus.PendingBasicInfo, "PendingBasicInfo")]
    [InlineData(EProviderStatus.PendingDocumentVerification, "PendingDocumentVerification")]
    [InlineData(EProviderStatus.Active, "Active")]
    [InlineData(EProviderStatus.Suspended, "Suspended")]
    public void ToString_ShouldReturnCorrectName(EProviderStatus status, string expectedName)
    {
        // Act
        var result = status.ToString();

        // Assert
        result.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(0, EProviderStatus.None)]
    [InlineData(1, EProviderStatus.PendingBasicInfo)]
    [InlineData(2, EProviderStatus.PendingDocumentVerification)]
    [InlineData(3, EProviderStatus.Active)]
    [InlineData(4, EProviderStatus.Suspended)]
    public void Cast_FromInt_ShouldReturnCorrectStatus(int value, EProviderStatus expectedStatus)
    {
        // Act
        var status = (EProviderStatus)value;

        // Assert
        status.Should().Be(expectedStatus);
    }

    [Fact]
    public void RegistrationFlow_ShouldFollowCorrectOrder()
    {
        // Arrange - Expected registration flow progression
        var expectedFlow = new[]
        {
            EProviderStatus.None,
            EProviderStatus.PendingBasicInfo,
            EProviderStatus.PendingDocumentVerification,
            EProviderStatus.Active
        };

        // Assert - Verify flow is in ascending order
        for (int i = 0; i < expectedFlow.Length - 1; i++)
        {
            ((int)expectedFlow[i]).Should().BeLessThan((int)expectedFlow[i + 1]);
        }
    }
}
