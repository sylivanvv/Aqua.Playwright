namespace Aqua.Framework.Utils;

public static class RandomGenerator
{
    private const string Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Digits = "0123456789";
    private const string LettersAndDigits = Letters + Digits;

    /// <summary>
    /// Returns a random decimal in the inclusive range [<paramref name="min"/>, <paramref name="max"/>],
    /// rounded to <paramref name="rounding"/> decimal places.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="min"/>
    /// is greater than <paramref name="max"/>.</exception>
    public static decimal GetRandomDecimal(decimal min, decimal max, int rounding = 2)
    {
        if (min > max)
            throw new ArgumentOutOfRangeException(nameof(min), min, $"min ({min}) cannot be greater than max ({max}).");
        if (min == max) return Math.Round(min, rounding);
        var range = (double)(max - min);
        var randomDouble = Random.Shared.NextDouble() * range;
        return Math.Round((decimal)randomDouble + min, rounding);
    }

    /// <summary>
    /// Returns a random money-style decimal in [<paramref name="min"/>, <paramref name="max"/>].
    /// </summary>
    /// <param name="min">min possible value</param>
    /// <param name="max">max possible value</param>
    /// <param name="isRound">
    /// When <c>true</c>, rounds the result to a whole number instead of the usual 2 decimal places.
    /// </param>
    public static decimal GetRandomMoney(decimal min, decimal max, bool isRound = false) =>
        isRound ? Math.Round(GetRandomDecimal(min, max), 0) : GetRandomDecimal(min, max);

    /// <summary>
    /// Returns a random integer in the inclusive range [<paramref name="min"/>, <paramref name="max"/>].
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="min"/>
    /// is greater than <paramref name="max"/>.</exception>
    public static int GetRandomInt(int min, int max) =>
        min <= max
            ? Random.Shared.Next(min, max + 1)
            : throw new
                ArgumentOutOfRangeException(nameof(min), min, $"min ({min}) cannot be greater than max ({max}).");

    public static int GetRandomInt(int min, int max, int except)
    {
        if (min > max)
            throw new ArgumentOutOfRangeException(nameof(min), min, $"min ({min}) cannot be greater than max ({max}).");
        if (except < min || except > max)
            return GetRandomInt(min, max);
        if (min == max)
            return min == except
                ? throw new ArgumentException($"Cannot exclude only value {except} in range [{min}, {max}] :")
                : min;

        var randomInt = Random.Shared.Next(min, max);
        return randomInt >= except ? randomInt + 1 : randomInt;
    }
    
    /// <summary>
    /// Decimal-bound convenience overload of <see cref="GetRandomInt(int, int)"/>: rounds
    /// <paramref name="min"/>/<paramref name="max"/> to the nearest integer and delegates to it
    /// </summary>
    public static int GetRandomInt(decimal min, decimal max) =>
        GetRandomInt(Convert.ToInt32(min), Convert.ToInt32(max));
    
    /// <summary>Returns a random alphabetic string of the given length.</summary>
    public static string RandomString(int length) => GenerateString(length, Letters);
    
    /// <summary>Returns a random alphanumeric string of the given length.</summary>
    public static string RandomStringWithNumbers(int length) => GenerateString(length, LettersAndDigits);
    
    /// <summary> Returns a random string of digits of the given length. </summary>
    public static string RandomStringNumber(int length) => GenerateString(length, Digits);

    private static string GenerateString(int length, string charset) =>
        length switch
        {
            < 0 => throw new ArgumentOutOfRangeException(nameof(length), length, "Length cannot be negative."),
            0 => string.Empty,
            _ => string.Create(length, charset, (span, allowedChars) =>
            {
                for (var i = 0; i < span.Length; i++) 
                    span[i] = allowedChars[Random.Shared.Next(allowedChars.Length)];
            })
        };

    public static string GenerateStringWithLabel(int length = 4) => $"[A] - {RandomString(length)}";

    public static string CreateNameForObject(string objectName, int length = 4) =>
        $"[A] {objectName} - {RandomString(length)}";
}