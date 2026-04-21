using MeAjudaAi.Shared.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_ShouldSetValues_WhenValid()
    {
        // Act
        var money = new Money(100.50m, "BRL");

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAmountIsNegative()
    {
        // Act
        var act = () => new Money(-1, "BRL");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_ShouldThrow_WhenCurrencyIsInvalid(string? currency)
    {
        // Act
        var act = () => new Money(10, currency!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldNormalizeCurrency()
    {
        // Act
        var money = new Money(10, " usd ");

        // Assert
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCurrencyNotSupported()
    {
        // Act
        var act = () => new Money(10, "XYZ");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*not supported*");
    }

    [Fact]
    public void Add_ShouldSumAmounts_WhenCurrenciesMatch()
    {
        // Arrange
        var m1 = new Money(10, "BRL");
        var m2 = new Money(20, "BRL");

        // Act
        var result = m1 + m2;

        // Assert
        result.Amount.Should().Be(30);
        result.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Add_ShouldThrow_WhenCurrenciesDoNotMatch()
    {
        // Arrange
        var m1 = new Money(10, "BRL");
        var m2 = new Money(10, "USD");

        // Act
        var act = () => m1 + m2;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Zero_ShouldReturnCorrectValue()
    {
        // Act
        var zero = Money.Zero("EUR");

        // Assert
        zero.Amount.Should().Be(0);
        zero.Currency.Should().Be("EUR");
    }

    [Fact]
    public void FromDecimal_ShouldReturnCorrectValue()
    {
        // Act
        var money = Money.FromDecimal(150.75m, "GBP");

        // Assert
        money.Amount.Should().Be(150.75m);
        money.Currency.Should().Be("GBP");
    }

    [Fact]
    public void IsSupported_ShouldReturnTrue_ForCommonCurrencies()
    {
        Money.IsSupported("BRL").Should().BeTrue();
        Money.IsSupported("USD").Should().BeTrue();
        Money.IsSupported("EUR").Should().BeTrue();
    }

    [Fact]
    public void IsSupported_ShouldReturnTrue_ForZeroDecimalCurrencies()
    {
        Money.IsSupported("JPY").Should().BeTrue();
        Money.IsSupported("KRW").Should().BeTrue();
    }

    [Fact]
    public void IsSupported_ShouldReturnTrue_ForThreeDecimalCurrencies()
    {
        Money.IsSupported("KWD").Should().BeTrue();
        Money.IsSupported("BHD").Should().BeTrue();
    }

    [Fact]
    public void IsSupported_ShouldReturnFalse_ForUnknownCurrencies()
    {
        Money.IsSupported("XYZ").Should().BeFalse();
        Money.IsSupported("").Should().BeFalse();
        Money.IsSupported(null!).Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectFormat()
    {
        // Act
        var money = new Money(1234.56m, "USD");

        // Assert
        money.ToString().Should().Be("USD 1234.56");
    }
}
