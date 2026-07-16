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
[AllureFeature("Favorites List")]
public class FavoriteGoodsTest : BasePlaywrightTest
{
    private GoodsListPage _goodsListPage;

    [SetUp]
    [AllureBefore("Open Catalog Page")]
    public async Task SetUp() => _goodsListPage = await Page.OpenPageAsync<GoodsListPage>(QaBrainsUrlCreator.QaBrainsBase);

    [Test]
    [Category(TestCategories.QaBrains)]
    [AllureStory("User can add goods to favorites")]
    [AllureName("Add goods to favorites and verify them on Favorites page")]
    public async Task TestAbilityToAddInFavorites()
    {
         var selectedGoodsItems = await AllureApi.Step("1. Select random goods from the catalog", async () =>
        {
            var allRows = await _goodsListPage.GoodsTable.GetRowsAsync();
            var selectedGoods = allRows.TakeRandomElementsAndKeepOrder().ToList();
            var itemsArray = await Task.WhenAll(selectedGoods.Select(r => r.AsDataAsync()));
            var rows = itemsArray.ToList();
            foreach (var row in selectedGoods)
                await row.AddGoodsInFavAsync();
            return rows;
        });
        await AllureApi.Step("2. Verify selected goods are present in Favorites", async () =>
        {
            var favoritesPage = await _goodsListPage.OpenUserFavoriteGoodsAsync();
            await favoritesPage.GoodsTable.WaitUntilRowsChangedAsync(async rows =>
            {
                if (rows.Count != selectedGoodsItems.Count)
                    return false;
                var results = await Task.WhenAll(rows.Select(row => row.IsGoodsAddedToFavAsync()));
                return results.All(isFav => isFav);
            });
            var actualGoods = await favoritesPage.GoodsTable.GetAllDataAsync<GoodsRowModel>();
            ExtendedAssertions.AreCollectionsEqual(selectedGoodsItems, actualGoods);
        });
    }
}