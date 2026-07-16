using Allure.Net.Commons;
using Allure.NUnit.Attributes;
using Aqua.Automation.AuthHelpers;
using Aqua.Automation.QaBrains.PageObjects;
using Aqua.Automation.QaBrains.PageObjects.Tables;
using Aqua.Automation.QaBrains.PageObjects.UrlCreator;
using Aqua.Automation.Utils;
using Aqua.Framework.Components.Table;
using Aqua.Framework.Extensions;
using Aqua.Framework.Utils;
using FluentAssertions;

namespace Aqua.Automation.QaBrains.UiTests;

[Parallelizable(ParallelScope.All)]
[WithAuth(AuthRole.QaBrainsUser)]
[AllureFeature("Items in Cart")]
public class CartTest : BasePlaywrightTest
{
    private GoodsListPage _goodsListPage;

    [SetUp]
    [AllureBefore("Open Catalog Page")]
    public async Task SetUp() =>
        _goodsListPage = await Page.OpenPageAsync<GoodsListPage>(QaBrainsUrlCreator.QaBrainsBase);

    [Test]
    [Category(TestCategories.QaBrains)]
    [AllureStory("Add to Cart")]
    [AllureName("Verify items added from catalog appear correctly in the cart")]
    public async Task AddItemsToCartTest()
    {
        var expectedGoodsData = await AllureApi.Step("1. Act: Add random items and check badge", async () =>
        {
            var randomGoodsRows = await _goodsListPage.GoodsTable.GetRandomRowsAsync();
            var rows = await randomGoodsRows.GetAllDataAsync();
            foreach (var row in randomGoodsRows) await row.AddGoodsInCartAsync();
            var actualCartCount = await _goodsListPage.GetItemsInCartCountAsync();
            ExtendedAssertions.AreEqual(randomGoodsRows.Count, actualCartCount, "Badge count should match");
            return rows;
        });
        await AllureApi.Step("2. Assert: Verify cart table contains correct data", async () =>
        {
            var cartPage = await _goodsListPage.OpenUserCartAsync();
            var actualCartData = await cartPage.GoodsTableInCart.GetAllDataAsync<CartRowModel>();
            expectedGoodsData.Should().BeEquivalentTo(actualCartData, options => options.ExcludingMissingMembers());
        });
    }

    [Test]
    [Category(TestCategories.QaBrains)]
    [AllureStory("Quantity Management")]
    [AllureName("Verify price recalculation when increasing and decreasing quantity")]
    public async Task ChangeItemQuantityAndVerifyMathTest()
    {
        var increaseBy = 0;
        var cartPage = await AllureApi.Step("1. Arrange: Prepare cart with items",
            async () => await AddRandomItemsAndOpenCartAsync());

        var rowToModify = await AllureApi.Step("2. Act & Assert: Increase quantity and check math", async () =>
        {
            var row = await cartPage.GoodsTableInCart.GetRandomRowAsync();
            var basePrice = (await row.Price.InnerTextAsync()).ToPrice();

            increaseBy = RandomGenerator.GetRandomInt(3, 5);
            var updatedRow = await row.AddQuantityAsync(increaseBy);

            var quantity = (await updatedRow.Quantity.InnerTextAsync()).ToPrice();
            var total = (await updatedRow.Total.InnerTextAsync()).ToPrice();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(quantity, Is.EqualTo(1 + increaseBy),
                    "Quantity should increase");
                Assert.That(total, Is.EqualTo(basePrice * quantity),
                    "Total should equal Price * Quantity");
            }

            return row;
        });

        await AllureApi.Step("3. Act & Assert: Decrease quantity and check math", async () =>
        {
            var basePrice = (await rowToModify.Price.InnerTextAsync()).ToPrice();
            var currentQuantity = (await rowToModify.Quantity.InnerTextAsync()).ToPrice();
            var decreaseAmount = RandomGenerator.GetRandomInt(2, increaseBy);
            var updatedRow = await rowToModify.ReduceQuantityAsync(decreaseAmount);

            var newQuantity = (await updatedRow.Quantity.InnerTextAsync()).ToPrice();
            var newTotal = (await updatedRow.Total.InnerTextAsync()).ToPrice();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(newQuantity, Is.EqualTo(currentQuantity - decreaseAmount),
                    "Quantity should decrease");
                Assert.That(newTotal, Is.EqualTo(basePrice * newQuantity),
                    "Total should recalculate");
            }
        });
    }

    [Test]
    [Category(TestCategories.QaBrains)]
    [AllureStory("Remove Cart Items")]
    [AllureName("Remove a single item from the cart")]
    public async Task RemoveSingleItemFromCartTest()
    {
        var cartPage = await AllureApi.Step("1. Arrange: Prepare cart with items",
            async () => await AddRandomItemsAndOpenCartAsync(uniqueElements: true));

        await AllureApi.Step("2. Act & Assert: Delete item and verify it disappears", async () =>
        {
            var rowToDelete = await cartPage.GoodsTableInCart.GetRandomRowAsync();
            var deletedItemData = await rowToDelete.AsDataAsync();
            await rowToDelete.RemoveItemFromCartAsync();
            var isRowStillPresent =
                await cartPage.GoodsTableInCart.IsRowPresentAsync(async row =>
                    await row.AsDataAsync() == deletedItemData);
            Assert.That(isRowStillPresent, Is.False, "Deleted item should not be present in cart");
        });
    }

    [Test]
    [Category(TestCategories.QaBrains)]
    [AllureStory("Remove Cart Items")]
    [AllureName("Clear the entire cart item by item")]
    public async Task RemoveAllItemsFromCartTest()
    {
        var cartPage = await AllureApi.Step("1. Arrange: Prepare test data in cart",
            async () => await AddRandomItemsAndOpenCartAsync());
        await AllureApi.Step("2. Act: Remove items one by one", async () =>
        {
            var goodsInCart = cartPage.GoodsTableInCart;
            var rowsCount = await goodsInCart.GetRowsCountAsync();
            for (var i = 0; i < rowsCount; i++)
            {
                var row = goodsInCart.GetFirstRow();
                await row.RemoveItemFromCartAsync();
            }
        });
        await AllureApi.Step("3. Assert: Verify cart is empty", async () =>
        {
            var isTableEmpty = await cartPage.GoodsTableInCart.IsTableEmptyAsync();
            Assert.That(isTableEmpty, Is.True, "Cart table should be empty");
        });
    }

    private async Task<CartPage> AddRandomItemsAndOpenCartAsync(int? count = null, bool uniqueElements = false)
    {
        var randomGoodsRows = !uniqueElements
            ? await _goodsListPage.GoodsTable.GetRandomRowsAsync(count)
            : await _goodsListPage.GoodsTable
                .GetUniqueRandomRowsAsync(r => r.GoodsName.InnerTextAsync(), count);
        foreach (var row in randomGoodsRows)
            await row.AddGoodsInCartAsync();
        return await _goodsListPage.OpenUserCartAsync();
    }
}