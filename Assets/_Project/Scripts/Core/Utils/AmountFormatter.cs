using System;
using System.Globalization;

namespace WheelOfVertigo.Core.Utils
{
    /// <summary>Short reward amounts for small labels: 950, 1.2K, 10K, 3.4M.</summary>
    public static class AmountFormatter
    {
        private const int Thousand = 1_000;
        private const int Million = 1_000_000;

        public static string Format(int amount)
        {
            if (amount < Thousand) return amount.ToString(CultureInfo.InvariantCulture);
            if (amount < Million) return Shorten(amount, Thousand, "K");
            return Shorten(amount, Million, "M");
        }

        public static string FormatMultiplier(int amount) => "x" + Format(amount);

        // Rounds down, so a label never shows more than the player really has.
        private static string Shorten(int amount, int unit, string suffix)
        {
            double value = Math.Floor(amount * 10.0 / unit) / 10.0;
            return value.ToString("0.#", CultureInfo.InvariantCulture) + suffix;
        }
    }
}
