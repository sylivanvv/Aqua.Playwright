namespace Aqua.Framework.Components.Table;

public static class TableRowExtensions
{
    public static async Task<List<TModel>> GetAllDataAsync<TModel>(this IEnumerable<IComparableRow<TModel>> rows)
    {
        var dataTasks = rows.Select(row => row.AsDataAsync());
        var results = await Task.WhenAll(dataTasks);
        return results.ToList();
    }
}