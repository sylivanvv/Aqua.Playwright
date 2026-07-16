using System.Runtime.CompilerServices;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;
using Aqua.Framework.Extensions;
using Aqua.Framework.Core;

namespace Aqua.Framework.Components;
/// <summary>
/// Wrapper around a custom dropdown UI component providing typed interaction methods.
/// <para>
/// Supports two dropdown archetypes:
/// <list type="bullet">
///   <item><description>
///     <b>Custom dropdown</b> — a clickable trigger that reveals a list of option elements.
///     Optionally includes a search input to filter options before selecting.
///     Use <see cref="SelectOptionAsync(string)"/>, <see cref="SelectOptionAsync(int)"/>,
///     <see cref="SelectRandomOptionAsync"/>, or <see cref="SelectOptionViaKeyboardAsync"/>.
///   </description></item>
///   <item><description>
///     <b>Native &lt;select&gt; element</b> — uses Playwright's built-in <c>SelectOptionAsync</c>.
///     Use <see cref="SelectRandomOptionInSelectAsync"/> for this case.
///   </description></item>
/// </list>
/// </para>
/// </summary>
/// <param name="dropdownLocator">
/// Locator for the dropdown trigger element (the button or container that opens the options list).
/// For native <c>&lt;select&gt;</c> elements, this is the <c>&lt;select&gt;</c> itself.
/// </param>
/// <param name="optionsLocator">
/// Locator for the individual option elements shown after the dropdown is opened.
/// For native <c>&lt;select&gt;</c>, this should target the <c>&lt;option&gt;</c> elements.
/// </param>
/// <param name="name">
/// Name for this dropdown instance, used in log messages and error descriptions.
/// </param>
/// <param name="searchLocator">
/// Optional locator for a search/filter input inside the dropdown.
/// When provided, <see cref="SelectOptionAsync(string)"/> types the option text into this field
/// before clicking, and <see cref="SelectOptionViaKeyboardAsync"/> navigates from this input.
/// </param>
public class Dropdown(
    ILocator dropdownLocator,
    ILocator optionsLocator,
    ILocator? searchLocator = null,
    [CallerMemberName] string name = null!)
{
    /// <summary>
    /// Logger instance for this dropdown, created lazily on first access.
    /// Log category is set to the concrete subclass name for easy filtering.
    /// </summary>
    protected ILogger Log => field ??= AquaServices.LoggerFactory.CreateLogger(GetType().Name);
    
    /// <summary>
    /// Opens the dropdown by clicking the trigger and waits until at least one option is visible.
    /// Called internally before any option interaction to ensure the options list is ready.
    /// </summary>
    private async Task WaitForOptionsAsync()
    {
        await dropdownLocator.ClickAsync();
        await optionsLocator.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }
    
    /// <summary>
    /// Opens the dropdown and selects the option that exactly matches the given text.
    /// If a <b>searchLocator</b> was provided, types the text into the search field first
    /// to filter the options list before clicking.
    /// </summary>
    /// <param name="optionToSelect">The exact visible text of the option to select.</param>
    /// <returns>The text of the selected option.</returns>
    public async Task<string> SelectOptionAsync(string optionToSelect)
    {
        Log.Info($"Select option '{optionToSelect}' in '{name}'");
        await WaitForOptionsAsync();

        if (searchLocator != null)
            await searchLocator.FillAsync(optionToSelect);
        var optionToClick = optionsLocator.GetByText(optionToSelect, new LocatorGetByTextOptions { Exact = true });
        var selectedText = await optionToClick.InnerTextAsync();
        await optionToClick.ClickAsync();
        return selectedText;
    }
    
    /// <summary>
    /// Opens the dropdown and selects the option at the given zero-based index.
    /// </summary>
    /// <param name="index">Zero-based index of the option to select.</param>
    /// <returns>The visible text of the selected option.</returns>
    public async Task<string> SelectOptionAsync(int index)
    {
        Log.Info($"Select option by index '{index}' in '{name}'");
        await WaitForOptionsAsync();

        var optionToClick = optionsLocator.Nth(index);
        var selectedText = await optionToClick.InnerTextAsync();
        await optionToClick.ClickAsync();
        
        return selectedText;
    }
        
    /// <summary>
    /// Opens the dropdown and selects a randomly chosen option from the full options list.
    /// Useful for randomized test data without a specific target value.
    /// </summary>
    /// <returns>The visible text of the randomly selected option.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the dropdown contains no options.</exception>
    public async Task<string> SelectRandomOptionAsync()
    {
        Log.Info($"Select random option in '{name}'");
        await WaitForOptionsAsync();

        var count = await optionsLocator.CountAsync();
        if (count == 0) throw new InvalidOperationException($"No options found in dropdown '{name}'");

        var randomIndex = Random.Shared.Next(count);
        var randomOption = optionsLocator.Nth(randomIndex);

        var selectedText = await randomOption.InnerTextAsync();
        await randomOption.ClickAsync();

        return selectedText;
    }
    
    /// <summary>
    /// Selects a random option from a native HTML <c>&lt;select&gt;</c> element
    /// using Playwright's built-in value-based selection.
    /// <para>
    /// Does not click to open — works directly with the <c>&lt;select&gt;</c> DOM API.
    /// Use this instead of <see cref="SelectRandomOptionAsync"/> for native select elements.
    /// </para>
    /// </summary>
    /// <param name="startIndex">
    /// Zero-based index to start random selection from. Defaults to <c>1</c> to skip
    /// the first option, which is typically a placeholder like "— Select —".
    /// </param>
    /// <returns>The <c>value</c> attribute of the selected <c>&lt;option&gt;</c> element.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no options are found, or if the selected option has no <c>value</c> attribute.
    /// </exception>
    public async Task<string> SelectRandomOptionInSelectAsync(int startIndex = 1)
    {
        Log.Info($"Select random option in '{name}'");
        var count = await optionsLocator.CountAsync();
        if (count <= 0) throw new InvalidOperationException($"No options found in dropdown '{name}'");

        var randomIndex = Random.Shared.Next(startIndex, count);
        var randomOption = await optionsLocator.Nth(randomIndex).GetAttributeAsync("value");

        await dropdownLocator.SelectOptionAsync(randomOption ?? throw new InvalidOperationException());
        return randomOption;
    }
    
    /// <summary>
    /// Opens the dropdown and navigates to the target option using keyboard arrow keys,
    /// then confirms the selection with Enter.
    /// <para>
    /// Use when the dropdown does not support click-based selection reliably or as workaround for unstable clicks
    /// </para>
    /// <para>
    /// If a <b>searchLocator</b> was provided, keyboard navigation starts from
    /// the search input. Otherwise, navigation starts from the dropdown trigger itself.
    /// </para>
    /// </summary>
    /// <param name="optionToSelect">The text of the option to navigate to and select.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no option containing <paramref name="optionToSelect"/> is found in the list.
    /// </exception>
    public async Task SelectOptionViaKeyboardAsync(string optionToSelect)
    {
        Log.Info($"Select option '{optionToSelect}' via keyboard in '{name}'");
        await WaitForOptionsAsync();

        var allTexts = await optionsLocator.AllInnerTextsAsync();
        var targetIndex = allTexts
            .Select(t => t.Trim())
            .ToList()
            .FindIndex(t => t.Contains(optionToSelect));

        if (targetIndex == -1)
            throw new InvalidOperationException($"Option '{optionToSelect}' was not found in dropdown.");

        var targetElementForKeyboard = searchLocator ?? dropdownLocator;

        for (var i = 0; i <= targetIndex; i++)
        {
            await targetElementForKeyboard.PressAsync("ArrowDown");
        }
        await targetElementForKeyboard.PressAsync("Enter");
    }
}