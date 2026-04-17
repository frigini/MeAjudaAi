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
    [InlineData("clp", true)]
    [InlineData("krw", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("UNKNOWN", false)]
    public void IsZeroDecimalCurrency_ShouldReturnCorrectValue(string? currency, bool expected)
    {
        CurrencyUtils.IsZeroDecimalCurrency(currency!).Should().Be(expected);
    }

    [Theory]
    [InlineData(99.90, "BRL", 9990)]
    [InlineData(10.00, "USD", 1000)]
    [InlineData(1000, "JPY", 1000)]
    [InlineData(100.50, "JPY", 101)] // Arredondamento para zero-decimal
    [InlineData(100.90, "JPY", 101)] // Arredondamento para zero-decimal
    [InlineData(-10.50, "BRL", -1050)]
    [InlineData(-10.50, "JPY", -11)] // Arredondamento simétrico para negativos
    [InlineData(1000.001, "KWD", 1000001)] // Three-decimal (Kuwaiti Dinar)
    [InlineData(1000000000.99, "USD", 100000000099)] // Valor grande
    public void ConvertToMinorUnits_ShouldReturnCorrectValue(decimal amount, string currency, long expected)
    {
        CurrencyUtils.ConvertToMinorUnits(amount, currency).Should().Be(expected);
    }

    [Theory]
    [InlineData(9990, "BRL", 99.90)]
    [InlineData(1000, "USD", 10.00)]
    [InlineData(1000, "JPY", 1000.00)]
    [InlineData(-1050, "BRL", -10.50)]
    [InlineData(1000001, "KWD", 1000.001)]
    [InlineData(100000000099, "USD", 1000000000.99)]
    public void ConvertFromMinorUnits_ShouldReturnCorrectValue(long amount, string currency, decimal expected)
    {
        CurrencyUtils.ConvertFromMinorUnits(amount, currency).Should().Be(expected);
    }
}
