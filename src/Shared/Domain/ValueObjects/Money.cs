using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Shared.Domain.ValueObjects;

public record Money
{
    public const string DefaultCurrency = "BRL";
    
    private static readonly HashSet<string> CommonCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "BRL", "USD", "EUR", "GBP", "CAD", "AUD", "CHF"
    };

    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
        
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        var normalized = currency.Trim().ToUpperInvariant();
        
        if (!IsSupported(normalized))
            throw new ArgumentException($"Currency '{normalized}' is not supported.", nameof(currency));

        Amount = amount;
        Currency = normalized;
    }

    public static bool IsSupported(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency)) return false;
        var normalized = currency.Trim();
        return CommonCurrencies.Contains(normalized) || 
               CurrencyUtils.IsZeroDecimalCurrency(normalized) || 
               CurrencyUtils.IsThreeDecimalCurrency(normalized);
    }

    public static Money Zero(string currency = DefaultCurrency) => new(0, currency);

    public static Money FromDecimal(decimal amount, string currency = DefaultCurrency) => new(amount, currency);

    public override string ToString() => $"{Currency} {Amount:N2}";

    public static Money operator +(Money a, Money b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies.");
        return new Money(a.Amount + b.Amount, a.Currency);
    }
}
