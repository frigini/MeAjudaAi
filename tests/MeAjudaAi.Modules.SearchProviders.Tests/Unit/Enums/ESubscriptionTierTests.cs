using FluentAssertions;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Enums;

public sealed class ESubscriptionTierTests
{
    [Fact]
    public void SubscriptionTier_ShouldHaveCorrectValues()
    {
        // Assert
        ((int)ESubscriptionTier.Free).Should().Be(0);
        ((int)ESubscriptionTier.Standard).Should().Be(1);
        ((int)ESubscriptionTier.Gold).Should().Be(2);
        ((int)ESubscriptionTier.Platinum).Should().Be(3);
    }

    [Fact]
    public void SubscriptionTier_ShouldHaveAllExpectedMembers()
    {
        // Arrange
        var expectedTiers = new[]
        {
            ESubscriptionTier.Free,
            ESubscriptionTier.Standard,
            ESubscriptionTier.Gold,
            ESubscriptionTier.Platinum
        };

        // Act
        var actualTiers = Enum.GetValues<ESubscriptionTier>();

        // Assert
        actualTiers.Should().BeEquivalentTo(expectedTiers);
        actualTiers.Should().HaveCount(4);
    }

    [Theory]
    [InlineData(ESubscriptionTier.Free, "Free")]
    [InlineData(ESubscriptionTier.Standard, "Standard")]
    [InlineData(ESubscriptionTier.Gold, "Gold")]
    [InlineData(ESubscriptionTier.Platinum, "Platinum")]
    public void ToString_ShouldReturnCorrectName(ESubscriptionTier tier, string expectedName)
    {
        // Act
        var result = tier.ToString();

        // Assert
        result.Should().Be(expectedName);
    }

    [Fact]
    public void SubscriptionTiers_ShouldBeOrderedByPriority()
    {
        // Arrange & Act
        var tiers = new[]
        {
            ESubscriptionTier.Free,
            ESubscriptionTier.Standard,
            ESubscriptionTier.Gold,
            ESubscriptionTier.Platinum
        };

        // Assert - Higher tier value = Higher priority
        ((int)tiers[0]).Should().BeLessThan((int)tiers[1]);
        ((int)tiers[1]).Should().BeLessThan((int)tiers[2]);
        ((int)tiers[2]).Should().BeLessThan((int)tiers[3]);
    }

    [Theory]
    [InlineData(0, ESubscriptionTier.Free)]
    [InlineData(1, ESubscriptionTier.Standard)]
    [InlineData(2, ESubscriptionTier.Gold)]
    [InlineData(3, ESubscriptionTier.Platinum)]
    public void Cast_FromInt_ShouldReturnCorrectTier(int value, ESubscriptionTier expectedTier)
    {
        // Act
        var tier = (ESubscriptionTier)value;

        // Assert
        tier.Should().Be(expectedTier);
    }
}
