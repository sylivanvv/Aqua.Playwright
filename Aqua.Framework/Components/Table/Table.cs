using System.Runtime.CompilerServices;
using Aqua.Framework.Core;
using Aqua.Framework.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Aqua.Framework.Components.Table;

/// <summary>
/// Generic wrapper around an HTML table locator that provides typed access to table rows.
/// <para>
/// Accepts a <typeparamref name="TTableRow"/> factory to instantiate strongly-typed row objects
/// from individual row locators, keeping DOM interaction encapsulated inside row classes.
/// </para>
/// <para>
/// Usage pattern:
/// <code>
/// public Table&lt;ExampleTableRow&gt; ExampleTable =>  new Table&lt;ExampleTableRow&gt;(
///     page.Locator("#example-table"),
///     locator => new ExampleTableRow(locator)
/// );
/// var rows = await table.GetRowsAsync();
/// </code>
/// </para>
/// </summary>
/// <typeparam name="TTableRow">
/// The strongly-typed row class. Must inherit from <see cref="BaseTableRow"/>.
/// Implement <see cref="IComparableRow{TModel}"/> on the row class to enable <see cref="GetAllDataAsync{TModel}"/>.
/// </typeparam>
public class Table<TTableRow> where TTableRow : BaseTableRow
{
    /// <summary>
    /// Logger instance for this table, created lazily on first access.
    /// Log category is set to the concrete subclass name for easy filtering.
    /// </summary>
    private ILogger Log => field ??=
        AquaServices.LoggerFactory.CreateLogger($"{GetType().Name.Split('`')[0]}<{typeof(TTableRow).Name}>");

    /// <summary>
    /// Locator for all row elements within the table.
    /// Scoped under tableLocator to avoid matching rows from other tables on the page.
    /// </summary>
    private readonly ILocator _rowsLocator;

    ///<summary>  Name of the current table when it is declared as property </summary>
    private readonly string _name;
    
    /// <summary>
    /// Factory delegate that wraps a row <see cref="ILocator"/> into a typed <typeparamref name="TTableRow"/> instance.
    /// Called for each row locator returned by Playwright to produce the strongly-typed row objects.
    /// </summary>
    private readonly Func<ILocator, TTableRow> _rowFactory;

    /// <summary>
    /// Initializes the table wrapper with a root locator and a row factory.
    /// </summary>
    /// <param name="tableLocator">Locator pointing to the root <c>&lt;table&gt;</c> element.</param>
    /// <param name="rowFactory">
    /// Factory that creates a typed row object from a Playwright locator.
    /// Typically: <c>locator => new MyTableRow(locator)</c>.
    /// </param>
    /// <param name="rowSelector">
    /// CSS selector used to find row elements within the table. Defaults to <c>"tr"</c>.
    /// Override for tables that use custom row markup (e.g. <c>"[data-row]"</c>).
    /// </param>
    /// <param name="name">
    /// Takes name of the Table property for logging purposes
    /// </param>
    public Table(ILocator tableLocator, Func<ILocator, TTableRow> rowFactory,
        string rowSelector, [CallerMemberName] string name = null!)
    {
        _rowFactory = rowFactory;
        _rowsLocator = tableLocator.Locator(rowSelector);
        _name = name;
    }

    /// <summary>
    /// Returns all present table rows as typed <typeparamref name="TTableRow"/> list of objects.
    /// Each row is wrapped using the <see cref="_rowFactory"/> delegate.
    /// </summary>
    public async Task<IReadOnlyList<TTableRow>> GetRowsAsync()
    {
        var rowLocators = await _rowsLocator.AllAsync();
        return rowLocators.Select(_rowFactory).ToList();
    }
    
    /// <summary>Returns the current number of rows in the table.</summary>
    public async Task<int> GetRowsCountAsync() => await _rowsLocator.CountAsync();

    /// <summary>
    /// Extracts data from all rows as a list of <typeparamref name="TModel"/> objects.
    /// Requires <typeparamref name="TTableRow"/> to implement <see cref="IComparableRow{TModel}"/>.
    /// Throws at runtime if the constraint is not satisfied.
    /// </summary>
    /// <typeparam name="TModel">The data model type that each row maps to.</typeparam>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <typeparamref name="TTableRow"/> does not implement <see cref="IComparableRow{TModel}"/>.
    /// </exception>
    public async Task<List<TModel>> GetAllDataAsync<TModel>()
    {
        var rows = await GetRowsAsync();
        if (rows is IEnumerable<IComparableRow<TModel>> typedRows)
            return await typedRows.GetAllDataAsync();
        throw new InvalidOperationException(
            $"{typeof(TTableRow).Name} doesn't implement IComparableRow<{typeof(TModel).Name}>");
    }

    /// <summary>
    /// Finds a single row containing the specified text.
    /// If multiple rows match, returns the first one
    /// </summary>
    /// <param name="textQuery">Text to search for within row content.</param>
    /// <exception cref="InvalidOperationException">Thrown if no matching row is found.</exception>
    public async Task<TTableRow> GetRowAsync(string textQuery)
    {
        Log.Info($"Get row by text '{textQuery}' in '{_name}'");
        var foundItems = _rowsLocator.Filter(new LocatorFilterOptions { HasText = textQuery });
        var foundItemsCount = await foundItems.CountAsync();
        return foundItemsCount != 0
            ? _rowFactory(foundItems.First)
            : throw new InvalidOperationException($"Row with text '{textQuery}' not found");
    }
    
    /// <summary>
    /// Returns a row matching the given Playwright filter options without fetching all rows.
    /// </summary>
    /// <param name="options">Playwright filter options (HasText, Has, etc.).</param>
    public TTableRow GetRow(LocatorFilterOptions options)
    {
        Log.Info($"Get table row by predicate {options} in '{_name}'");
        var rowLocator = _rowsLocator.Filter(options);
        return _rowFactory(rowLocator);
    }
    
    /// <summary>
    /// Returns the row at the given zero-based index using Playwright's <c>Nth()</c> locator.
    /// </summary>
    /// <param name="rowIndex">Zero-based row index.</param>
    public TTableRow GetRow(int rowIndex)
    {
        Log.Info($"Get table row by '{rowIndex}' index in '{_name}'");
        var rowLocator = _rowsLocator.Nth(rowIndex);
        return _rowFactory(rowLocator);
    }
    
    /// <summary>
    /// Returns the first row in the table using Playwright's <c>First</c> locator.
    /// </summary>
    public TTableRow GetFirstRow()
    {
        Log.Info($"Get First table row in {_name}");
        return _rowFactory(_rowsLocator.First);
    }
    
    /// <summary>
    /// Returns the last row in the table using Playwright's <c>Last</c> locator.
    /// Useful for verifying newly appended rows without knowing the total count.
    /// </summary>
    public TTableRow GetLastRow()
    {
        Log.Info($"Get Last table row in {_name}");
        return _rowFactory(_rowsLocator.Last);
    }
    
    /// <summary>
    /// Returns all rows that match the given Playwright filter options.
    /// </summary>
    /// <param name="options">Playwright filter options applied to the rows locator.</param>
    public async Task<List<TTableRow>> FilterRowsAsync(LocatorFilterOptions options)
    {
        Log.Info($"Filter table rows by predicate in '{_name}'");
        var filteredLocator = _rowsLocator.Filter(options);
        var individualLocators = await filteredLocator.AllAsync();
        return individualLocators.Select(_rowFactory).ToList();
    }
    
    /// <summary>
    /// Returns a single randomly selected row from all rows.
    /// </summary>
    /// <exception cref="Exception">Thrown if the table has no rows.</exception>
    public async Task<TTableRow> GetRandomRowAsync()
    {
        Log.Info($"Get random table row in '{_name}'");
        var count = await _rowsLocator.CountAsync();
        return count != 0 ? _rowFactory(_rowsLocator.Nth(Random.Shared.Next(count))) 
            : throw new InvalidOperationException($"Table '{_name}' is empty");
    }

    /// <summary>
    /// Returns a random subset of rows from the table.
    /// Waits for at least the first row to be visible before fetching.
    /// </summary>
    /// <param name="count">
    /// Number of rows to return. Pass <c>null</c> (default) to return a random number of rows.
    /// </param>
    public async Task<List<TTableRow>> GetRandomRowsAsync(int? count = null)
    {
        await _rowsLocator.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        Log.Info($"Get {count} random table rows in '{_name}'");
        var rows = await GetRowsAsync();
        return rows.TakeRandomElements(count);
    }
    
    /// <summary>
    /// Returns a subset of randomly selected rows guaranteed to be unique by the given key.
    /// Waits for at least the first row to be visible before fetching.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of the key used to determine uniqueness (e.g. <c>string</c> for name, <c>int</c> for ID).
    /// </typeparam>
    /// <param name="asyncKeySelector">
    /// Async function that extracts a unique key from each row.
    /// Used to deduplicate results — rows with the same key are never both selected.
    /// Example: <c>row => row.GetNameAsync()</c>
    /// </param>
    /// <param name="numberOfElements">
    /// Number of unique rows to return.
    /// Pass <c>null</c> (default) to return a random number of rows.
    /// </param>
    /// <returns>A list of randomly selected rows with no duplicate keys.</returns>
    public async Task<List<TTableRow>> GetUniqueRandomRowsAsync<TKey>(Func<TTableRow, Task<TKey>> asyncKeySelector, int? numberOfElements = null)
    {
        await _rowsLocator.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        Log.Info($"Get {numberOfElements} random table rows in '{_name}'");
        var rows = await GetRowsAsync();
        return await rows.TakeUniqueRandomElementsAsync(asyncKeySelector, numberOfElements);
    }
    
    /// <summary>
    /// Returns a random row from the subset of rows matching the given filter options.
    /// Useful for selecting a random row from a filtered view without fetching all rows first.
    /// </summary>
    /// <param name="options">Playwright filter options to narrow the candidate rows.</param>
    /// <exception cref="InvalidOperationException">Thrown if no rows match the filter.</exception>
    public async Task<TTableRow> GetRandomRowAsync(LocatorFilterOptions options)
    {
        Log.Info($"Get random table row by {options} in '{_name}'");  
        var filteredLocator = _rowsLocator.Filter(options);
        var count = await filteredLocator.CountAsync();
        return count == 0 ? throw new InvalidOperationException($"No rows found matching the predicate in '{_name}'")
            : _rowFactory(filteredLocator.Nth(Random.Shared.Next(count)));
    }
    
    /// <summary>
    /// Returns the zero-based index of the first row satisfying the predicate,
    /// or <c>-1</c> if no matching row is found.
    /// </summary>
    /// <param name="predicate">Async function evaluated against each row in order.</param>
    public async Task<int> GetIndexAsync(Func<TTableRow, Task<bool>> predicate)
    {
        var rows = await GetRowsAsync();
        for (var i = 0; i < rows.Count; i++)
            if (await predicate(rows[i]))
                return i;
        return -1;
    }
    
    /// <summary>
    /// Returns <c>true</c> if at least one row contains the specified text.
    /// Uses Playwright's built-in text filter — does not fetch all rows.
    /// </summary>
    /// <param name="rowQuery">Text to search for within row content.</param>
    public async Task<bool> IsRowPresentAsync(string rowQuery)
    {
        Log.Info($"Check if row '{rowQuery}' is present in '{_name}'");
        var targetRow = _rowsLocator.Filter(new LocatorFilterOptions { HasText = rowQuery });
        return await targetRow.CountAsync() > 0;
    }
    
    /// <summary>
    /// Returns <c>true</c> if at least one row satisfies the given predicate.
    /// Fetches all rows and evaluates the predicate against each one sequentially.
    /// </summary>
    /// <param name="predicate">Async function evaluated against each row until a match is found.</param>
    public async Task<bool> IsRowPresentAsync(Func<TTableRow, Task<bool>> predicate)
    {
        var rows = await GetRowsAsync();
        foreach (var row in rows)
            if (await predicate(row))
                return true;
        return false;
    }

    /// <summary>Returns <c>true</c> if the table contains no rows.</summary>
    public async Task<bool> IsTableEmptyAsync()
    {
        var rowCount = await _rowsLocator.CountAsync();
        return rowCount == 0;
    }
    
    /// <summary>
    /// Waits until the number of rows changes from its current (or provided) value.
    /// Useful for waiting after an action that adds or removes rows (e.g. delete, pagination).
    /// </summary>
    /// <param name="sec">Maximum wait time in seconds. Defaults to 45.</param>
    /// <param name="startCount">
    /// The row count to wait for a change from.
    /// Pass <c>0</c> (default) to capture the current count automatically before waiting.
    /// Pass an explicit value to avoid an extra DOM query when the count is already known.
    /// </param>
    public async Task WaitUntilRowsCountChangesAsync(int sec = 45, int? startCount = null)
    {
        Log.Info($"Wait until rows count changes in '{_name}'");
        var oldCount = startCount ?? await _rowsLocator.CountAsync();
        await Assertions.Expect(_rowsLocator).Not.ToHaveCountAsync(oldCount, new LocatorAssertionsToHaveCountOptions
        {
            Timeout = sec * 1000
        });
    }
    
    /// <summary>
    /// Polls the table until the string value produced by <paramref name="predicate"/> changes
    /// from its initial value, or until the timeout is reached.
    /// </summary>
    /// <param name="predicate">
    /// Function that extracts a string snapshot from the current rows.
    /// Called once to capture the initial value, then repeatedly until the value changes.
    /// </param>
    /// <param name="sec">Maximum polling duration in seconds. Defaults to 15.</param>
    /// <param name="cancellationToken">Token used to cooperatively abandon waiting.</param>
    /// <exception cref="TimeoutException">Thrown if the value does not change within the timeout.</exception>
    public async Task WaitUntilRowsChangedAsync(Func<IReadOnlyList<TTableRow>, Task<string>> predicate, int sec = 15,
        CancellationToken cancellationToken = default)
    {
        Log.Info($"Wait until rows change in '{_name}'");
        var initialRows = await GetRowsAsync();
        var oldValue = await predicate(initialRows);
        await PollUntilAsync(async () =>
            {
                var currentRows = await GetRowsAsync();
                var currentValue = await predicate(currentRows);
                return currentValue != oldValue;
            }, sec, $"Rows in '{_name}' did not change within {sec}s", cancellationToken);
    }
    
    /// <summary>
    /// Polls the table until the given <paramref name="predicate"/> returns <c>true</c>,
    /// or until the timeout is reached.
    /// <para>
    /// Use when waiting for a specific row to appear, disappear, or change state.
    /// <see cref="PlaywrightException"/> is suppressed during polling to handle transient DOM detachment.
    /// </para>
    /// </summary>
    /// <param name="predicate">
    /// Async condition evaluated against the current rows on each polling cycle.
    /// Return <c>true</c> to stop waiting.
    /// </param>
    /// <param name="sec">Maximum polling duration in seconds. Defaults to 15.</param>
    /// <param name="cancellationToken">Token used to cooperatively abandon waiting.</param>
    /// <exception cref="TimeoutException">Thrown if the predicate never returns <c>true</c> within the timeout.</exception>
    public async Task WaitUntilRowsChangedAsync(Func<IReadOnlyList<TTableRow>, Task<bool>> predicate, int sec = 15,
        CancellationToken cancellationToken = default)
    {
        Log.Info($"Wait until rows change in '{_name}'");
        await PollUntilAsync(async () =>
            {
                var rows = await GetRowsAsync();
                return await predicate(rows);
            }, sec, $"Condition not met for rows in '{_name}' within {sec}s", cancellationToken);
    }
    
    private static async Task PollUntilAsync(
        Func<Task<bool>> condition, int sec, string timeoutMessage, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow.AddSeconds(sec);
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (await condition()) return;
            }
            catch (PlaywrightException) { }

            await Task.Delay(300, cancellationToken);
        }
        throw new TimeoutException(timeoutMessage);
    }
}