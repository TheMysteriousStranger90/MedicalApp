using System.Globalization;

namespace Medical.Client.Helpers;

public static class CurrencyHelper
{
    private static readonly CultureInfo USDCulture = new CultureInfo("en-US");

    public static string FormatUSD(double amount)
    {
        return amount.ToString("C", USDCulture);
    }
}