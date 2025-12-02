using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Domain.ValueObjects;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Domain")]
public sealed class AddressTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateAddress()
    {
        // Arrange
        var street = "Avenida Paulista";
        var number = "1000";
        var neighborhood = "Bela Vista";
        var city = "São Paulo";
        var state = "SP";
        var zipCode = "01310-100";

        // Act
        var address = new Address(street, number, neighborhood, city, state, zipCode);

        // Assert
        address.Should().NotBeNull();
        address.Street.Should().Be("Avenida Paulista");
        address.Number.Should().Be("1000");
        address.Neighborhood.Should().Be("Bela Vista");
        address.City.Should().Be("São Paulo");
        address.State.Should().Be("SP");
        address.ZipCode.Should().Be("01310-100");
        address.Country.Should().Be("Brazil");
        address.Complement.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithComplement_ShouldIncludeComplement()
    {
        // Arrange
        var complement = "Apto 101";

        // Act
        var address = new Address("Rua A", "100", "Centro", "São Paulo", "SP", "01000-000", complement: complement);

        // Assert
        address.Complement.Should().Be("Apto 101");
    }

    [Fact]
    public void Constructor_WithCustomCountry_ShouldUseCustomCountry()
    {
        // Arrange
        var country = "Portugal";

        // Act
        var address = new Address("Rua A", "100", "Centro", "Lisboa", "LX", "1000-001", country);

        // Assert
        address.Country.Should().Be("Portugal");
    }

    [Fact]
    public void Constructor_WithWhitespace_ShouldTrimAllFields()
    {
        // Act
        var address = new Address(
            "  Rua A  ",
            "  100  ",
            "  Centro  ",
            "  São Paulo  ",
            "  SP  ",
            "  01000-000  ",
            "  Brazil  ",
            "  Apto 101  ");

        // Assert
        address.Street.Should().Be("Rua A");
        address.Number.Should().Be("100");
        address.Neighborhood.Should().Be("Centro");
        address.City.Should().Be("São Paulo");
        address.State.Should().Be("SP");
        address.ZipCode.Should().Be("01000-000");
        address.Country.Should().Be("Brazil");
        address.Complement.Should().Be("Apto 101");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidStreet_ShouldThrowArgumentException(string? invalidStreet)
    {
        // Act
        var act = () => new Address(invalidStreet!, "100", "Centro", "São Paulo", "SP", "01000-000");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Street cannot be empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidNumber_ShouldThrowArgumentException(string? invalidNumber)
    {
        // Act
        var act = () => new Address("Rua A", invalidNumber!, "Centro", "São Paulo", "SP", "01000-000");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Number cannot be empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidNeighborhood_ShouldThrowArgumentException(string? invalidNeighborhood)
    {
        // Act
        var act = () => new Address("Rua A", "100", invalidNeighborhood!, "São Paulo", "SP", "01000-000");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Neighborhood cannot be empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCity_ShouldThrowArgumentException(string? invalidCity)
    {
        // Act
        var act = () => new Address("Rua A", "100", "Centro", invalidCity!, "SP", "01000-000");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("City cannot be empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidState_ShouldThrowArgumentException(string? invalidState)
    {
        // Act
        var act = () => new Address("Rua A", "100", "Centro", "São Paulo", invalidState!, "01000-000");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("State cannot be empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidZipCode_ShouldThrowArgumentException(string? invalidZipCode)
    {
        // Act
        var act = () => new Address("Rua A", "100", "Centro", "São Paulo", "SP", invalidZipCode!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("ZipCode cannot be empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCountry_ShouldThrowArgumentException(string? invalidCountry)
    {
        // Act
        var act = () => new Address("Rua A", "100", "Centro", "São Paulo", "SP", "01000-000", invalidCountry!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Country cannot be empty*");
    }

    [Fact]
    public void ToString_WithoutComplement_ShouldReturnFormattedAddress()
    {
        // Arrange
        var address = new Address("Avenida Paulista", "1000", "Bela Vista", "São Paulo", "SP", "01310-100");

        // Act
        var result = address.ToString();

        // Assert
        result.Should().Be("Avenida Paulista, 1000, Bela Vista, São Paulo/SP, 01310-100, Brazil");
    }

    [Fact]
    public void ToString_WithComplement_ShouldIncludeComplement()
    {
        // Arrange
        var address = new Address("Avenida Paulista", "1000", "Bela Vista", "São Paulo", "SP", "01310-100", complement: "Apto 101");

        // Act
        var result = address.ToString();

        // Assert
        result.Should().Be("Avenida Paulista, 1000, Apto 101, Bela Vista, São Paulo/SP, 01310-100, Brazil");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var address1 = new Address("Rua A", "100", "Centro", "São Paulo", "SP", "01000-000");
        var address2 = new Address("Rua A", "100", "Centro", "São Paulo", "SP", "01000-000");

        // Act & Assert
        address1.Should().Be(address2);
        (address1 == address2).Should().BeTrue();
        (address1 != address2).Should().BeFalse();
        address1.GetHashCode().Should().Be(address2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentStreet_ShouldNotBeEqual()
    {
        // Arrange
        var address1 = new Address("Rua A", "100", "Centro", "São Paulo", "SP", "01000-000");
        var address2 = new Address("Rua B", "100", "Centro", "São Paulo", "SP", "01000-000");

        // Act & Assert
        address1.Should().NotBe(address2);
    }

    [Fact]
    public void Equals_WithDifferentNumber_ShouldNotBeEqual()
    {
        // Arrange
        var address1 = new Address("Rua A", "100", "Centro", "São Paulo", "SP", "01000-000");
        var address2 = new Address("Rua A", "200", "Centro", "São Paulo", "SP", "01000-000");

        // Act & Assert
        address1.Should().NotBe(address2);
    }

    [Fact]
    public void Equals_WithDifferentZipCode_ShouldNotBeEqual()
    {
        // Arrange
        var address1 = new Address("Rua A", "100", "Centro", "São Paulo", "SP", "01000-000");
        var address2 = new Address("Rua A", "100", "Centro", "São Paulo", "SP", "02000-000");

        // Act & Assert
        address1.Should().NotBe(address2);
    }

    [Fact]
    public void Equals_WithDifferentComplement_ShouldNotBeEqual()
    {
        // Arrange
        var address1 = new Address("Rua A", "100", "Centro", "São Paulo", "SP", "01000-000", complement: "Apto 101");
        var address2 = new Address("Rua A", "100", "Centro", "São Paulo", "SP", "01000-000", complement: "Apto 102");

        // Act & Assert
        address1.Should().NotBe(address2);
    }

    [Fact]
    public void Equals_OneWithComplementOneWithout_ShouldNotBeEqual()
    {
        // Arrange
        var address1 = new Address("Rua A", "100", "Centro", "São Paulo", "SP", "01000-000", complement: "Apto 101");
        var address2 = new Address("Rua A", "100", "Centro", "São Paulo", "SP", "01000-000");

        // Act & Assert
        address1.Should().NotBe(address2);
    }
}
