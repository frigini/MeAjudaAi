using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Shared.Domain.ValueObjects;

public record Money
{
    public const string DefaultCurrency = "BRL";
    
    private static readonly HashSet<string> SupportedCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "BRL", "USD", "EUR", "GBP", "CAD", "AUD", "CHF",
        // Zero-decimal
        "BIF", "CLP", "DJF", "GNF", "JPY", "KMF", "KRW", "MGA", "PYG", "RWF", "UGX", "VND", "VUV", "XAF", "XOF", "XPF",
        // Three-decimal
        "BHD", "JOD", "KWD", "OMR", "TND"
    };

    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentNullException(nameof(currency), "Currency cannot be null or empty.");

        var normalized = currency.Trim().ToUpperInvariant();
        
        if (!SupportedCurrencies.Contains(normalized))
            throw new ArgumentException($"Currency '{normalized}' is not supported.", nameof(currency));

        Amount = amount;
        Currency = normalized;
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
