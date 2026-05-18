using System.Globalization;

namespace Medical.Client.Helpers;

public static class CurrencyHelper
{
    private static readonly CultureInfo USDCulture = new("en-US");

    public static string FormatUSD(double amount) => amount.ToString("C", USDCulture);
}
