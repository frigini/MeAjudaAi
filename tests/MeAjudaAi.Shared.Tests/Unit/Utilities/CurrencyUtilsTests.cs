using MeAjudaAi.Shared.Utilities;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Utilities;

public class CurrencyUtilsTests
{
    [Theory]
    [InlineData("BRL", false)]
    [InlineData("USD", false)]
    [InlineData("JPY", true)]
    [InlineData("CLP", true)]
    [InlineData("KRW", true)]
    [InlineData("jpy", true)]
    public void IsZeroDecimalCurrency_ShouldReturnCorrectValue(string currency, bool expected)
    {
        CurrencyUtils.IsZeroDecimalCurrency(currency).Should().Be(expected);
    }

    [Theory]
    [InlineData(99.90, "BRL", 9990)]
    [InlineData(10.00, "USD", 1000)]
    [InlineData(1000, "JPY", 1000)]
    [InlineData(100.50, "JPY", 101)] // Arredondamento para zero-decimal
    [InlineData(100.90, "JPY", 101)] // Arredondamento para zero-decimal
    public void ConvertToMinorUnits_ShouldReturnCorrectValue(decimal amount, string currency, long expected)
    {
        CurrencyUtils.ConvertToMinorUnits(amount, currency).Should().Be(expected);
    }

    [Theory]
    [InlineData(9990, "BRL", 99.90)]
    [InlineData(1000, "USD", 10.00)]
    [InlineData(1000, "JPY", 1000.00)]
    public void ConvertFromMinorUnits_ShouldReturnCorrectValue(long amount, string currency, decimal expected)
    {
        CurrencyUtils.ConvertFromMinorUnits(amount, currency).Should().Be(expected);
    }
}
