using MeAjudaAi.Shared.Domain.ValueObjects;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Payments;

[ExcludeFromCodeCoverage]
public static class MoneyBuilder
{
    public static Money Brl(decimal amount) => Money.FromDecimal(amount, "BRL");
    public static Money Usd(decimal amount) => Money.FromDecimal(amount, "USD");
    public static Money Eur(decimal amount) => Money.FromDecimal(amount, "EUR");
    public static Money Gbp(decimal amount) => Money.FromDecimal(amount, "GBP");
    public static Money Cad(decimal amount) => Money.FromDecimal(amount, "CAD");
    public static Money Aud(decimal amount) => Money.FromDecimal(amount, "AUD");
    public static Money Chf(decimal amount) => Money.FromDecimal(amount, "CHF");

    public static Money ZeroBrl() => Money.Zero("BRL");
    public static Money ZeroUsd() => Money.Zero("USD");
    public static Money ZeroEur() => Money.Zero("EUR");

    public static Money FromDecimal(decimal amount, string currency = "BRL") => Money.FromDecimal(amount, currency);
}
