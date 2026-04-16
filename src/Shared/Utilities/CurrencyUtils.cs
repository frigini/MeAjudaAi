namespace MeAjudaAi.Shared.Utilities;

public static class CurrencyUtils
{
    private static readonly HashSet<string> ZeroDecimalCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "BIF", "CLP", "DJF", "GNF", "JPY", "KMF", "KRW", "MGA", "PYG", "RWF", "UGX", "VND", "VUV", "XAF", "XOF", "XPF"
    };

    public static bool IsZeroDecimalCurrency(string currency)
    {
        return !string.IsNullOrWhiteSpace(currency) && ZeroDecimalCurrencies.Contains(currency.Trim());
    }

    public static decimal ConvertFromMinorUnits(long minorUnits, string currency)
    {
        if (IsZeroDecimalCurrency(currency))
        {
            return minorUnits;
        }

        return minorUnits / 100m;
    }
    
    public static long ConvertToMinorUnits(decimal amount, string currency)
    {
        if (IsZeroDecimalCurrency(currency))
        {
            // Moedas zero-decimal não aceitam frações
            return (long)Math.Round(amount);
        }

        return (long)Math.Round(amount * 100);
    }
}
