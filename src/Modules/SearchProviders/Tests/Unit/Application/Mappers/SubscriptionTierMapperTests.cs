using MeAjudaAi.Contracts.Modules.SearchProviders.Enums;
using MeAjudaAi.Modules.SearchProviders.Application.Mappers;
using DomainEnums = MeAjudaAi.Modules.SearchProviders.Domain.Enums;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Application.Mappers;

[Trait("Category", "Unit")]
[Trait("Module", "SearchProviders")]
[Trait("Layer", "Application")]
public class SubscriptionTierMapperTests
{
    [Theory]
    [InlineData(ESubscriptionTier.Free, DomainEnums.ESubscriptionTier.Free)]
    [InlineData(ESubscriptionTier.Standard, DomainEnums.ESubscriptionTier.Standard)]
    [InlineData(ESubscriptionTier.Gold, DomainEnums.ESubscriptionTier.Gold)]
    [InlineData(ESubscriptionTier.Platinum, DomainEnums.ESubscriptionTier.Platinum)]
    public void ToDomainTier_WithValidTier_ShouldMapCorrectly(ESubscriptionTier input, DomainEnums.ESubscriptionTier expected)
    {
        // Act
        var result = input.ToDomainTier();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(DomainEnums.ESubscriptionTier.Free, ESubscriptionTier.Free)]
    [InlineData(DomainEnums.ESubscriptionTier.Standard, ESubscriptionTier.Standard)]
    [InlineData(DomainEnums.ESubscriptionTier.Gold, ESubscriptionTier.Gold)]
    [InlineData(DomainEnums.ESubscriptionTier.Platinum, ESubscriptionTier.Platinum)]
    public void ToModuleTier_WithValidTier_ShouldMapCorrectly(DomainEnums.ESubscriptionTier input, ESubscriptionTier expected)
    {
        // Act
        var result = input.ToModuleTier();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToDomainTier_ShouldBeRoundTrip()
    {
        // Arrange
        var moduleTiers = Enum.GetValues<ESubscriptionTier>();

        foreach (var moduleTier in moduleTiers)
        {
            // Act
            var domainTier = moduleTier.ToDomainTier();
            var roundTripped = domainTier.ToModuleTier();

            // Assert
            roundTripped.Should().Be(moduleTier, $"round trip failed for {moduleTier}");
        }
    }

    [Fact]
    public void ToModuleTier_ShouldBeRoundTrip()
    {
        // Arrange
        var domainTiers = Enum.GetValues<DomainEnums.ESubscriptionTier>();

        foreach (var domainTier in domainTiers)
        {
            // Act
            var moduleTier = domainTier.ToModuleTier();
            var roundTripped = moduleTier.ToDomainTier();

            // Assert
            roundTripped.Should().Be(domainTier, $"round trip failed for {domainTier}");
        }
    }

    [Fact]
    public void ToDomainTier_WithInvalidValue_ShouldThrowNotSupportedException()
    {
        // Arrange
        var invalidTier = (ESubscriptionTier)999;

        // Act
        var act = () => invalidTier.ToDomainTier();

        // Assert
        act.Should().Throw<NotSupportedException>()
            .WithMessage("*999*");
    }

    [Fact]
    public void ToModuleTier_WithInvalidValue_ShouldThrowNotSupportedException()
    {
        // Arrange
        var invalidTier = (DomainEnums.ESubscriptionTier)999;

        // Act
        var act = () => invalidTier.ToModuleTier();

        // Assert
        act.Should().Throw<NotSupportedException>()
            .WithMessage("*999*");
    }
}
