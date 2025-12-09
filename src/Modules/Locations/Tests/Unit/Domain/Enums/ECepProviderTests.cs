using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.Enums;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Domain.Enums;

public sealed class ECepProviderTests
{
    [Fact]
    public void CepProvider_ShouldHaveAllExpectedMembers()
    {
        // Arrange
        var expectedProviders = new[]
        {
            ECepProvider.ViaCep,
            ECepProvider.BrasilApi,
            ECepProvider.OpenCep
        };

        // Act
        var actualProviders = Enum.GetValues<ECepProvider>();

        // Assert
        actualProviders.Should().BeEquivalentTo(expectedProviders);
        actualProviders.Should().HaveCount(3);
    }

    [Theory]
    [InlineData(ECepProvider.ViaCep, "ViaCep")]
    [InlineData(ECepProvider.BrasilApi, "BrasilApi")]
    [InlineData(ECepProvider.OpenCep, "OpenCep")]
    public void ToString_ShouldReturnCorrectName(ECepProvider provider, string expectedName)
    {
        // Act
        var result = provider.ToString();

        // Assert
        result.Should().Be(expectedName);
    }

    [Fact]
    public void Enum_ShouldBeDefined()
    {
        // Arrange
        var providers = new[]
        {
            ECepProvider.ViaCep,
            ECepProvider.BrasilApi,
            ECepProvider.OpenCep
        };

        // Act & Assert
        foreach (var provider in providers)
        {
            Enum.IsDefined(typeof(ECepProvider), provider).Should().BeTrue();
        }
    }

    [Fact]
    public void GetNames_ShouldReturnAllProviderNames()
    {
        // Act
        var names = Enum.GetNames<ECepProvider>();

        // Assert
        names.Should().Contain("ViaCep");
        names.Should().Contain("BrasilApi");
        names.Should().Contain("OpenCep");
        names.Should().HaveCount(3);
    }
}
