using System.Globalization;

namespace Aqua.Automation.Utils;

public static partial class StringExtensions
{
    extension(string text)
    {
        public decimal ToPrice()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(text);

            var cleanString = PriceCleanupRegex().Replace(text, "");

            var lastDot = cleanString.LastIndexOf('.');
            var lastComma = cleanString.LastIndexOf(',');

            if (lastDot >= 0 && lastComma >= 0)
            {
                cleanString = lastDot > lastComma
                    ? cleanString.Replace(",", "") // (US): "1,234.56" = "1234.56"
                    : cleanString.Replace(".", "").Replace(",", "."); // (EU): "1.234,56" = "1234.56"
            }
            else if (lastComma >= 0)
            {
                var charsAfterComma = cleanString.Length - lastComma - 1;
                cleanString = charsAfterComma == 3
                    ? cleanString.Replace(",", "") // (US): 1,000 = 1000
                    : cleanString.Replace(",", "."); // (EU): 10,50 = 10.50
            }
            else if (lastDot >= 0)
            {
                var charsAfterDot = cleanString.Length - lastDot - 1;
                if (charsAfterDot == 3)
                    cleanString = cleanString.Replace(".", ""); // (EU) 3 = 1.000 - 1000
            }

            return decimal.TryParse(cleanString, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : throw new FormatException($"Impossible to extract price from this string '{text}'");
        }

        public int ToInt()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(text);

            var match = StringToIntRegex().Match(text);
            if (match.Success && int.TryParse(match.Value, out var result))
                return result;
            throw new FormatException($"Impossible to extract integer from string: '{text}'");
        }
    }

    [GeneratedRegex(@"[^\d.,-]")]
    private static partial Regex PriceCleanupRegex();

    [GeneratedRegex(@"-?\d+")]
    private static partial Regex StringToIntRegex();
}