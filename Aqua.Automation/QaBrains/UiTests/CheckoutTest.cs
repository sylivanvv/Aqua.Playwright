using Allure.Net.Commons;
using Allure.NUnit.Attributes;
using Aqua.Automation.AuthHelpers;
using Aqua.Automation.QaBrains.PageObjects;
using Aqua.Automation.QaBrains.PageObjects.Tables;
using Aqua.Automation.QaBrains.PageObjects.UrlCreator;
using Aqua.Automation.Utils;
using Aqua.Framework.Extensions;

namespace Aqua.Automation.QaBrains.UiTests;

[Parallelizable(ParallelScope.All)]
[WithAuth(AuthRole.QaBrainsUser)]
[AllureFeature("Checkout Process")]
public class CheckoutTest : BasePlaywrightTest
{
    private GoodsListPage _goodsListPage;
    private const decimal TaxRate = 0.05m;

    [SetUp]
    [AllureBefore("Open Catalog Page")]
    public async Task SetUp() =>
        _goodsListPage = await Page.OpenPageAsync<GoodsListPage>(QaBrainsUrlCreator.QaBrainsBase);

    [Test]
    [Category(TestCategories.QaBrains)]
    [AllureStory("Items in cart are shown in checkout correctly")]
    [AllureName("Verify cart items, quantity changes, and price calculations on checkout overview")]
    public async Task VerifyCheckoutCalculationsTest()
    {
        CheckoutOverviewPage checkoutOverviewPage = null!;
        var goodsInCart = await AllureApi.Step("1. Arrange: Add random items to cart and setup data for test",
            async () =>
            {
                var selectedGoods = await _goodsListPage.GoodsTable.GetRandomRowsAsync();
                foreach (var row in selectedGoods)
                    await row.AddGoodsInCartAsync();
                var cartPage = await _goodsListPage.OpenUserCartAsync();
                var randomRow = await cartPage.GoodsTableInCart.GetRandomRowAsync();
                await randomRow.AddQuantityAsync(3);
                var rows = await cartPage.GoodsTableInCart.GetAllDataAsync<CartRowModel>();
                var checkoutForm = await cartPage.OpenCheckoutFormAsync();
                checkoutOverviewPage = await checkoutForm.OpenCheckoutOverviewAsync();
                return rows;
            });
        await AllureApi.Step("2. Assert: Proceed to Checkout Overview", async () =>
        {
            var goodsInCheckout = await checkoutOverviewPage.GoodsTableInCheckout.GetAllDataAsync<CartRowModel>();
            ExtendedAssertions.AreCollectionsEqual(goodsInCart, goodsInCheckout);
            await VerifyCheckoutCalculations(checkoutOverviewPage, goodsInCheckout);
        });
    }

    private static async Task VerifyCheckoutCalculations(CheckoutOverviewPage checkoutOverviewPage,
        List<CartRowModel> goodsInCheckout)
    {
        var expectedSubtotal = goodsInCheckout.Sum(r => r.Total);
        var expectedTax = Math.Round(expectedSubtotal * TaxRate, 2, MidpointRounding.AwayFromZero);

        var actualSubtotal = await checkoutOverviewPage.GetAllItemsTotalAsync();
        var actualTax = await checkoutOverviewPage.GetTaxAsync();
        var actualGrandTotal = await checkoutOverviewPage.GetGrandTotalAsync();

        ExtendedAssertions.Multiple(() =>
        {
            ExtendedAssertions.AreEqual(expectedSubtotal, actualSubtotal,
                "All items total must be equal to sum of all items in cart");

            ExtendedAssertions.AreEqual(expectedTax, actualTax,
                $"Tax should be correctly calculated as {TaxRate * 100}% of Subtotal");

            ExtendedAssertions.AreEqual(actualSubtotal + actualTax, actualGrandTotal,
                "Grand total must be equal to Subtotal + Tax");
        });
    }
}