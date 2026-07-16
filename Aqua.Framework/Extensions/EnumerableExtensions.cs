namespace Aqua.Framework.Extensions;

public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        public T RandomElement()
        {
            ArgumentNullException.ThrowIfNull(source);
            if (source is IReadOnlyList<T> list)
                return list.Count == 0
                    ? throw new InvalidOperationException("Sequence contains no elements")
                    : list[Random.Shared.Next(list.Count)];
            var array = source.ToArray();
            return array.Length == 0
                ? throw new InvalidOperationException("Sequence contains no elements")
                : array[Random.Shared.Next(array.Length)];
        }

        public List<T> TakeRandomElements(int? numberOfElements = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            var array = source.ToArray();
            var count = GetAndValidateCount(numberOfElements, array.Length);
            Random.Shared.Shuffle(array);
            return [.. array.Take(count)];
        }

        public List<T> TakeRandomElementsAndKeepOrder(int? numberOfElements = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            var list = source as IReadOnlyList<T> ?? source.ToArray();
            var count = GetAndValidateCount(numberOfElements, list.Count);

            var result = new List<T>(count);
            var remainingToSelect = count;
            var remainingInList = list.Count;
            for (var i = 0; i < list.Count && remainingToSelect > 0; i++)
            {
                if (Random.Shared.Next(remainingInList) < remainingToSelect)
                {
                    result.Add(list[i]);
                    remainingToSelect--;
                }

                remainingInList--;
            }

            return result;
        }

        public List<T> TakeUniqueRandomElements<TKey>(Func<T, TKey> keySelector, int? numberOfElements = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);

            var uniqueArray = source.DistinctBy(keySelector).ToArray();
            var count = GetAndValidateCount(numberOfElements, uniqueArray.Length);
            Random.Shared.Shuffle(uniqueArray);
            return uniqueArray.Take(count).ToList();
        }

        public async Task<List<T>> TakeUniqueRandomElementsAsync<TKey>(Func<T, Task<TKey>> keySelector,
            int? numberOfElements = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);

            var enumerable = source.ToArray();

            var itemsWithKeys = await Task.WhenAll(
                enumerable.Select(async item => (
                    Item: item,
                    Key: await keySelector(item))));

            var uniqueArray = itemsWithKeys
                .DistinctBy(x => x.Key)
                .Select(x => x.Item)
                .ToArray();

            var count = GetAndValidateCount(numberOfElements, uniqueArray.Length);

            Random.Shared.Shuffle(uniqueArray);

            return uniqueArray.Take(count).ToList();
        }
    }

    private static int GetAndValidateCount(int? requested, int available)
    {
        if (requested < 0)
            throw new ArgumentOutOfRangeException(nameof(requested), "Number of elements cannot be negative.");

        var count = requested ?? (available == 0 ? 0 : Random.Shared.Next(1, available + 1));

        if (count > available)
            throw new ArgumentOutOfRangeException(
                $"Requested {count} elements, but sequence contains only {available}.");

        return count;
    }
}