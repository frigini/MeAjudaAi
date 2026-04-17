namespace MeAjudaAi.Shared.Utilities;

public static class CurrencyUtils
{
    private static readonly HashSet<string> ZeroDecimalCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "BIF", "CLP", "DJF", "GNF", "JPY", "KMF", "KRW", "MGA", "PYG", "RWF", "UGX", "VND", "VUV", "XAF", "XOF", "XPF"
    };

    private static readonly HashSet<string> ThreeDecimalCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "BHD", "JOD", "KWD", "OMR", "TND"
    };

    public static bool IsZeroDecimalCurrency(string currency)
    {
        return !string.IsNullOrWhiteSpace(currency) && ZeroDecimalCurrencies.Contains(currency.Trim());
    }

    public static bool IsThreeDecimalCurrency(string currency)
    {
        return !string.IsNullOrWhiteSpace(currency) && ThreeDecimalCurrencies.Contains(currency.Trim());
    }

    public static decimal ConvertFromMinorUnits(long minorUnits, string currency)
    {
        if (IsZeroDecimalCurrency(currency))
        {
            return minorUnits;
        }

        if (IsThreeDecimalCurrency(currency))
        {
            return minorUnits / 1000m;
        }

        return minorUnits / 100m;
    }
    
    public static long ConvertToMinorUnits(decimal amount, string currency)
    {
        if (IsZeroDecimalCurrency(currency))
        {
            // Moedas zero-decimal não aceitam frações
            return (long)Math.Round(amount, 0, MidpointRounding.AwayFromZero);
        }

        if (IsThreeDecimalCurrency(currency))
        {
            return (long)Math.Round(amount * 1000m, 0, MidpointRounding.AwayFromZero);
        }

        return (long)Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
    }
}
