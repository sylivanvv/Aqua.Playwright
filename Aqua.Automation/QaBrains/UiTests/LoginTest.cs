using Allure.Net.Commons;
using Allure.NUnit.Attributes;
using Aqua.Automation.QaBrains.PageObjects;
using Aqua.Automation.QaBrains.PageObjects.UrlCreator;
using Aqua.Automation.Utils;
using Aqua.Framework.Core;
using Aqua.Framework.Extensions;
using Aqua.Framework.Utils;
using static Microsoft.Playwright.Assertions;

namespace Aqua.Automation.QaBrains.UiTests;

[Parallelizable(ParallelScope.All)]
[AllureFeature("Login Functionality")]
public class LoginTest : BasePlaywrightTest
{
    private LoginPage _loginPage;
    private static string ValidEmail => AquaServices.Config.AuthData.QaBrainsUser.UserName;
    private static string ValidPassword => AquaServices.Config.AuthData.QaBrainsUser.Password;

    [SetUp]
    [AllureBefore("Open Login page")]
    public async Task SetUp() => _loginPage = await Page.OpenPageAsync<LoginPage>(QaBrainsUrlCreator.QaBrainsBase);

    [Test]
    [Category(TestCategories.QaBrains)]
    [AllureStory("Successful Login")]
    [AllureName("User should be able to login with valid credentials")]
    public async Task LoginWithValidCredentialsTest()
    {
        var mainPage = await AllureApi.Step($"Login with valid email: {ValidEmail}",
            async () => await _loginPage.LoginAsync(ValidEmail, ValidPassword));
        await AllureApi.Step("Verify successful login and user profile information",
            async () => await Expect(mainPage.UserProfile).ToHaveTextAsync(ValidEmail));
    }

    [Test]
    [Category(TestCategories.QaBrains)]
    [AllureStory("Unsuccessful Login")]
    [AllureName("Login fails with Invalid Email and Invalid Password")]
    public async Task LoginFailsWithBothInvalidCredentialsTest()
    {
        var randomEmail = RandomGenerator.RandomString(5) + "@gmail.com";
        var randomPass = RandomGenerator.RandomStringWithNumbers(8);
        Log.Info("Test data - Email: {Email}, Pass: {Pass}", randomEmail, randomPass);
        await AllureApi.Step("1. Act & Assert: Attempt to login with invalid email and password", async () =>
        {
            await _loginPage.LoginInvalidDataAsync(randomEmail, randomPass);
            await Expect(_loginPage.WrongDataGrowl,
                "Error message should be displayed for invalid credentials").ToBeVisibleAsync();
        });
    }

    [Test]
    [Category(TestCategories.QaBrains)]
    [AllureStory("Unsuccessful Login")]
    [AllureName("Login fails with Invalid Email")]
    public async Task LoginFailsWithInvalidEmailTest()
    {
        var randomEmail = RandomGenerator.RandomString(5) + "@gmail.com";
        Log.Info("Test data - Email: {Email}", randomEmail);
        await AllureApi.Step("1. Act & Assert: Attempt to login with invalid email and valid password", async () =>
        {
            await _loginPage.LoginInvalidDataAsync(randomEmail, ValidPassword);
            await Expect(_loginPage.WrongDataGrowl,
                "Error message should be displayed for invalid credentials").ToBeVisibleAsync();
        });
    }

    [Test]
    [Category(TestCategories.QaBrains)]
    [AllureStory("Unsuccessful Login")]
    [AllureName("Login fails with Valid Email and Invalid Password")]
    public async Task LoginFailsWithInvalidPasswordTest()
    {
        var randomPass = RandomGenerator.RandomStringWithNumbers(8);
        Log.Info("Test data - Pass: {Pass}", randomPass);
        await AllureApi.Step("1. Act & Assert: Attempt to login with valid email and invalid password", async () =>
        {
            await _loginPage.LoginInvalidDataAsync(ValidEmail, password: randomPass);
            await Expect(_loginPage.WrongDataGrowl,
                "Error message should be displayed for invalid credentials").ToBeVisibleAsync();
        });
    }
}