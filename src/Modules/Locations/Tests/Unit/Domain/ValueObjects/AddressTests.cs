using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.ValueObjects;
using MeAjudaAi.Shared.Geolocation;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Domain.ValueObjects;

public sealed class AddressTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnAddressObject()
    {
        // Arrange
        var cep = Cep.Create("01310100");
        var street = "Avenida Paulista";
        var neighborhood = "Bela Vista";
        var city = "São Paulo";
        var state = "SP";

        // Act
        var address = Address.Create(cep, street, neighborhood, city, state);

        // Assert
        address.Should().NotBeNull();
        address!.Cep.Should().Be(cep);
        address.Street.Should().Be("Avenida Paulista");
        address.Neighborhood.Should().Be("Bela Vista");
        address.City.Should().Be("São Paulo");
        address.State.Should().Be("SP");
        address.Complement.Should().BeNull();
        address.GeoPoint.Should().BeNull();
    }

    [Fact]
    public void Create_WithComplement_ShouldIncludeComplement()
    {
        // Arrange
        var cep = Cep.Create("01310100");
        var street = "Avenida Paulista";
        var neighborhood = "Bela Vista";
        var city = "São Paulo";
        var state = "SP";
        var complement = "Lado ímpar";

        // Act
        var address = Address.Create(cep, street, neighborhood, city, state, complement);

        // Assert
        address.Should().NotBeNull();
        address!.Complement.Should().Be("Lado ímpar");
    }

    [Fact]
    public void Create_WithGeoPoint_ShouldIncludeGeoPoint()
    {
        // Arrange
        var cep = Cep.Create("01310100");
        var street = "Avenida Paulista";
        var neighborhood = "Bela Vista";
        var city = "São Paulo";
        var state = "SP";
        var geoPoint = new GeoPoint(-23.561414, -46.656239);

        // Act
        var address = Address.Create(cep, street, neighborhood, city, state, geoPoint: geoPoint);

        // Assert
        address.Should().NotBeNull();
        address!.GeoPoint.Should().Be(geoPoint);
    }

    [Fact]
    public void Create_WithNullCep_ShouldReturnNull()
    {
        // Act
        var address = Address.Create(null, "Street", "Neighborhood", "City", "SP");

        // Assert
        address.Should().BeNull();
    }

    [Theory]
    [InlineData(null, "Neighborhood", "City", "SP")]
    [InlineData("", "Neighborhood", "City", "SP")]
    [InlineData("  ", "Neighborhood", "City", "SP")]
    public void Create_WithInvalidStreet_ShouldReturnNull(string? street, string neighborhood, string city, string state)
    {
        // Arrange
        var cep = Cep.Create("01310100");

        // Act
        var address = Address.Create(cep, street, neighborhood, city, state);

        // Assert
        address.Should().BeNull();
    }

    [Theory]
    [InlineData("Street", null, "City", "SP")]
    [InlineData("Street", "", "City", "SP")]
    [InlineData("Street", "  ", "City", "SP")]
    public void Create_WithInvalidNeighborhood_ShouldReturnNull(string street, string? neighborhood, string city, string state)
    {
        // Arrange
        var cep = Cep.Create("01310100");

        // Act
        var address = Address.Create(cep, street, neighborhood, city, state);

        // Assert
        address.Should().BeNull();
    }

    [Theory]
    [InlineData("Street", "Neighborhood", null, "SP")]
    [InlineData("Street", "Neighborhood", "", "SP")]
    [InlineData("Street", "Neighborhood", "  ", "SP")]
    public void Create_WithInvalidCity_ShouldReturnNull(string street, string neighborhood, string? city, string state)
    {
        // Arrange
        var cep = Cep.Create("01310100");

        // Act
        var address = Address.Create(cep, street, neighborhood, city, state);

        // Assert
        address.Should().BeNull();
    }

    [Theory]
    [InlineData("Street", "Neighborhood", "City", null)]
    [InlineData("Street", "Neighborhood", "City", "")]
    [InlineData("Street", "Neighborhood", "City", "   ")]
    [InlineData("Street", "Neighborhood", "City", "S")] // 1 letra
    [InlineData("Street", "Neighborhood", "City", "SAO")] // 3 letras
    public void Create_WithInvalidState_ShouldReturnNull(string street, string neighborhood, string city, string? state)
    {
        // Arrange
        var cep = Cep.Create("01310100");

        // Act
        var address = Address.Create(cep, street, neighborhood, city, state);

        // Assert
        address.Should().BeNull();
    }

    [Fact]
    public void Create_WithLowercaseState_ShouldNormalizeToUppercase()
    {
        // Arrange
        var cep = Cep.Create("01310100");

        // Act
        var address = Address.Create(cep, "Street", "Neighborhood", "City", "sp");

        // Assert
        address.Should().NotBeNull();
        address!.State.Should().Be("SP");
    }

    [Fact]
    public void ToString_WithoutComplement_ShouldReturnFormattedAddress()
    {
        // Arrange
        var cep = Cep.Create("01310100");
        var address = Address.Create(cep, "Avenida Paulista", "Bela Vista", "São Paulo", "SP");

        // Act
        var result = address!.ToString();

        // Assert
        result.Should().Be("Avenida Paulista, Bela Vista, São Paulo, SP, 01310-100");
    }

    [Fact]
    public void ToString_WithComplement_ShouldIncludeComplementInFormattedAddress()
    {
        // Arrange
        var cep = Cep.Create("01310100");
        var address = Address.Create(cep, "Avenida Paulista", "Bela Vista", "São Paulo", "SP", "Lado ímpar");

        // Act
        var result = address!.ToString();

        // Assert
        result.Should().Be("Avenida Paulista, Lado ímpar, Bela Vista, São Paulo, SP, 01310-100");
    }

    #region Equality Tests

    [Fact]
    public void Equals_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var cep = Cep.Create("12345678")!;
        var address1 = Address.Create(
            cep,
            "Rua das Flores",
            "Centro",
            "São Paulo",
            "SP");

        var address2 = Address.Create(
            cep,
            "Rua das Flores",
            "Centro",
            "São Paulo",
            "SP");

        // Act & Assert
        address1.Should().Be(address2);
        (address1 == address2).Should().BeTrue();
        (address1 != address2).Should().BeFalse();
        address1!.GetHashCode().Should().Be(address2!.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentStreet_ShouldNotBeEqual()
    {
        // Arrange
        var cep = Cep.Create("12345678")!;
        var address1 = Address.Create(
            cep,
            "Rua das Flores",
            "Centro",
            "São Paulo",
            "SP");

        var address2 = Address.Create(
            cep,
            "Rua das Palmeiras",
            "Centro",
            "São Paulo",
            "SP");

        // Act & Assert
        address1.Should().NotBe(address2);
        (address1 == address2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentCep_ShouldNotBeEqual()
    {
        // Arrange
        var address1 = Address.Create(
            Cep.Create("12345678")!,
            "Rua das Flores",
            "Centro",
            "São Paulo",
            "SP");

        var address2 = Address.Create(
            Cep.Create("87654321")!,
            "Rua das Flores",
            "Centro",
            "São Paulo",
            "SP");

        // Act & Assert
        address1.Should().NotBe(address2);
    }

    [Fact]
    public void Equals_WithComplement_ShouldConsiderInEquality()
    {
        // Arrange
        var cep = Cep.Create("12345678")!;
        var address1 = Address.Create(
            cep,
            "Rua das Flores",
            "Centro",
            "São Paulo",
            "SP",
            "Apto 101");

        var address2 = Address.Create(
            cep,
            "Rua das Flores",
            "Centro",
            "São Paulo",
            "SP",
            "Apto 102");

        var address3 = Address.Create(
            cep,
            "Rua das Flores",
            "Centro",
            "São Paulo",
            "SP");

        // Act & Assert
        address1.Should().NotBe(address2); // Different complements
        address1.Should().NotBe(address3); // One has complement, other doesn't
    }

    [Fact]
    public void Equals_WithGeoPoint_ShouldConsiderInEquality()
    {
        // Arrange
        var cep = Cep.Create("12345678")!;
        var geoPoint1 = new GeoPoint(-23.5505, -46.6333);
        var geoPoint2 = new GeoPoint(-22.9068, -43.1729);

        var address1 = Address.Create(
            cep,
            "Rua das Flores",
            "Centro",
            "São Paulo",
            "SP",
            geoPoint: geoPoint1);

        var address2 = Address.Create(
            cep,
            "Rua das Flores",
            "Centro",
            "São Paulo",
            "SP",
            geoPoint: geoPoint2);

        var address3 = Address.Create(
            cep,
            "Rua das Flores",
            "Centro",
            "São Paulo",
            "SP");

        // Act & Assert
        address1.Should().NotBe(address2); // Different geo points
        address1.Should().NotBe(address3); // One has geo point, other doesn't
    }

    [Fact]
    public void Equals_WithNull_ShouldHandleCorrectly()
    {
        // Arrange
        var address = Address.Create(
            Cep.Create("12345678")!,
            "Rua das Flores",
            "Centro",
            "São Paulo",
            "SP");

        // Act & Assert
        address.Should().NotBe(null);
        (address == null).Should().BeFalse();
        (address != null).Should().BeTrue();
    }

    [Fact]
    public void Equals_SameInstance_ShouldBeEqual()
    {
        // Arrange
        var address = Address.Create(
            Cep.Create("12345678")!,
            "Rua das Flores",
            "Centro",
            "São Paulo",
            "SP");

        // Act & Assert
        address.Should().Be(address);
        address!.Equals(address).Should().BeTrue();
#pragma warning disable CS1718 // Comparison made to same variable
        (address == address).Should().BeTrue();
#pragma warning restore CS1718
    }

    #endregion
}
