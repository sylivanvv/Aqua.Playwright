using Allure.Net.Commons;
using Allure.NUnit.Attributes;
using Aqua.Automation.AuthHelpers;
using Aqua.Automation.QaBrains.PageObjects;
using Aqua.Automation.QaBrains.PageObjects.UrlCreator;
using Aqua.Automation.Utils;
using Aqua.Framework.Extensions;

namespace Aqua.Automation.QaBrains.UiTests;

[Parallelizable(ParallelScope.All)]
[WithAuth(AuthRole.QaBrainsUser)]
[AllureFeature("Goods Sorting")]
public class GoodsSortingTest : BasePlaywrightTest
{
    private GoodsListPage _goodsListPage;

    [SetUp]
    [AllureBefore("Open Catalog Page")]
    public async Task SetUp() => _goodsListPage = await Page.OpenPageAsync<GoodsListPage>(QaBrainsUrlCreator.QaBrainsBase);

    [Test]
    [Category(TestCategories.QaBrains)]
    [AllureStory("User can sort goods by Price (Ascending and Descending)")]
    [AllureName("Verify sorting by Price functionality")]
    public async Task SortGoodsByPriceTest()
    {
        await AllureApi.Step("1. Select and check 'Price (Low to High)' in sorting dropdown", async () =>
        {
            await _goodsListPage.SortingSelection.SelectOptionAsync("Low to High (Price)");
            await CheckIfPriceSortedAsync(false);
        });
        await AllureApi.Step("2. Select and check 'Price (High to Low)' in sorting dropdown", async () =>
        {
            await _goodsListPage.SortingSelection.SelectOptionAsync("High to Low (Price)");
            await CheckIfPriceSortedAsync(true);
        });
    }

    private async Task CheckIfPriceSortedAsync(bool isDescending)
    {
        await _goodsListPage.GoodsTable.WaitUntilRowsChangedAsync(async rows =>
        {
            var priceTexts = await Task.WhenAll(rows.Select(r => r.Price.InnerTextAsync()));
            var prices = priceTexts.Select(p => p.ToPrice()).ToList();
            return isDescending
                ? prices.SequenceEqual(prices.OrderByDescending(x => x))
                : prices.SequenceEqual(prices.OrderBy(x => x));
        });
    } 
}