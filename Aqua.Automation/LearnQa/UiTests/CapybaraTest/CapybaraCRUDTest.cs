using Allure.Net.Commons;
using Allure.NUnit.Attributes;
using Aqua.Automation.AuthHelpers;
using Aqua.Automation.LearnQa.CapybaraApi;
using Aqua.Automation.LearnQa.Models.Capybara;
using Aqua.Automation.LearnQa.PageObjects.CapybarasPages;
using Aqua.Automation.LearnQa.PageObjects.UrlCreator;
using Aqua.Automation.Utils;
using Aqua.Framework.Core;
using Aqua.Framework.Extensions;
using Aqua.Framework.Utils;

namespace Aqua.Automation.LearnQa.UiTests.CapybaraTest;

[Parallelizable(ParallelScope.All)]
[AllureFeature("Capybara Features")]
[WithAuth(AuthRole.LearnQaUser)]
public class CapybaraCRUDTest : BaseLearnQaTest
{
    private CapybarasBasePage _capybarasPage;
    private CapybaraModel? _createdCapybara;
    private readonly List<CapybaraModel> _listOfCreatedCapybaras = [];

    protected CapybaraApiClient Client => field ??= AquaServices.Get<CapybaraApiClient>();

    [SetUp]
    [AllureBefore("Open Capybaras page and set UI context to capybara client")]
    public async Task SetUp()
    {
        _capybarasPage = await Page.OpenPageAsync<CapybarasBasePage>(LearnQaUrlCreator.CapybarasBasePage);
        Client.SetTraceContext(Page.APIRequest);
    }

    [TearDown]
    [AllureAfter("Cleanup: Delete created Capybaras using API")]
    public async Task DeleteCreatedCapybaraAsync()
    {
        if (_createdCapybara is not null)
            _listOfCreatedCapybaras.Add(_createdCapybara);
        foreach (var capybara in _listOfCreatedCapybaras)
        {
            try
            {
                capybara.Id ??= (await Client.GetCreatedCapybaraFromAPIAsync(capybara.Name)).Id;
                await Client.DeleteCapybara(capybara.Id);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to delete {capybara.Name}: {ex.Message}");
            }
        }
    }

    [Test]
    [Category(TestCategories.Capybara)]
    [AllureStory("User can create a new Capybara via UI")]
    [AllureName("Create Capybara using UI form")]
    public async Task CreateCapybaraUiTest()
    {
        AllureApi.Step("1. Arrange: Generate random Capybara data", () =>
        {
            _createdCapybara = CapybaraModel.CreateRandom();
            Log.Info("Generated Capybara model: {@Capybara}", _createdCapybara);
        });
        await AllureApi.Step("2. Act: Fill creation form and submit",
            async () => await CreateCapybaraUsingUIAsync(_createdCapybara!));
        await AllureApi.Step("3. Assert: Verify displayed info matches generated model", async () =>
            ExtendedAssertions.AreEqual(_createdCapybara, await _capybarasPage.GetCapybaraModelFromPageAsync(),
                "Info from randomized model and ui should be equal"));
    }

    [Test]
    [Category(TestCategories.Capybara)]
    [AllureStory("Data consistency between UI creation and API response")]
    [AllureName("Verify Capybara created via UI exists in API response")]
    public async Task CheckCapybaraApiAndUiSameData()
    {
        var randomCapybara = CapybaraModel.CreateRandom();
        Log.Info("Generated Capybara model: {@Capybara}", randomCapybara);
        await AllureApi.Step("1. Act: Create Capybara via UI",
            async () => await CreateCapybaraUsingUIAsync(randomCapybara));
        await AllureApi.Step("2. Assert: Verify Capybara details via API GET request", async () =>
        {
            _createdCapybara = await Client.GetCreatedCapybaraFromAPIAsync(randomCapybara.Name);
            randomCapybara.Id = _createdCapybara.Id;
            ExtendedAssertions.AreEqual(randomCapybara, _createdCapybara,
                "Info from api and ui should be equal");
        });
    }

    [Test]
    [Category(TestCategories.Capybara)]
    [AllureStory("Capybara could be created using API and be displayed in UI")]
    [AllureName("Create Capybara via API and verify its data on UI")]
    public async Task CreateCapybaraUsingAPIAndVerifyItsCreatedOnUI()
    {
        var randomCapybara = CapybaraModel.CreateRandom();
        Log.Info("Generated Capybara model: {@Capybara}", randomCapybara);
        await AllureApi.Step("1. Arrange: Create Capybara using API POST request",
            async () => _createdCapybara = await Client.CreateCapybara(randomCapybara));
        await AllureApi.Step("2. Act: Refresh UI to fetch new data", async () =>
        {
            _capybarasPage = await Page.ReloadAsync<CapybarasBasePage>();
            await _capybarasPage.SelectCapybaraViewAsync(randomCapybara.Name);
        });
        await AllureApi.Step("3. Assert: Verify UI displays correct data", async () =>
        {
            ExtendedAssertions.AreEqual(randomCapybara, await _capybarasPage.GetCapybaraModelFromPageAsync(),
                "Info from model and ui should be equal");
        });
    }

    [Test]
    [Category(TestCategories.Capybara)]
    [AllureStory("Capybara could be updated using API and should be displayed in UI")]
    [AllureName("Update Capybara via API and verify its data on UI")]
    public async Task UpdateCapybaraUsingAPIAndVerifyItsUpdatedOnUI()
    {
        CapybaraModel updatedCapybara = null!;
        await AllureApi.Step("1. Arrange: Create capybara using API", async () =>
        {
            var randomCapybara = CapybaraModel.CreateRandom();
            Log.Info("Old capybara: {@Capybara}", randomCapybara);
            var capybaraId = (await Client.CreateCapybara(randomCapybara)).Id;
            //we keep old capybara's id and age (it cannot be updated)
            updatedCapybara = CapybaraModel.CreateRandom() with { Age = randomCapybara.Age, Id = capybaraId };
            Log.Info("Updated capybara: {@Capybara}", updatedCapybara);
        });
        await AllureApi.Step("2. Act: Update capybara using API and reload page to fetch updates", async () =>
        {
            _createdCapybara = await Client.UpdateCapybara(updatedCapybara);
            _capybarasPage = await Page.ReloadAsync<CapybarasBasePage>();
            await _capybarasPage.SelectCapybaraViewAsync(_createdCapybara.Name);
        });
        await AllureApi.Step("3. Assert: Verify updated details on UI", async () =>
        {
            var capybaraFromUi = await _capybarasPage.GetCapybaraModelFromPageAsync();
            capybaraFromUi.Id = updatedCapybara.Id;
            ExtendedAssertions.AreEqual(_createdCapybara, capybaraFromUi,
                "Info from model and ui should be equal");
        });
    }

    [Test]
    [Category(TestCategories.Capybara)]
    [AllureStory("Capybaras can form friendships via API")]
    [AllureName("Make Capybara friends using API and validate relation")]
    public async Task CreateCapybaraFriendsUsingApi()
    {
        _createdCapybara = CapybaraModel.CreateRandom();
        await AllureApi.Step("1. Act: Create a Capybara and random friends", async () =>
        {
            _createdCapybara.Id = (await Client.CreateCapybara(_createdCapybara)).Id;
            var friendsCount = RandomGenerator.GetRandomInt(2, 4);
            for (var i = 0; i < friendsCount; i++)
            {
                var randomFriend = CapybaraModel.CreateRandom();
                _listOfCreatedCapybaras.Add(randomFriend);
                randomFriend.Id = (await Client.CreateCapybara(randomFriend)).Id;
                await Client.AddFriend(_createdCapybara.Id, randomFriend.Id);
            }
        });
        await AllureApi.Step("2. Assert: Validate friendship lists", async () =>
        {
            var friendsList = await Client.GetFriends(_createdCapybara.Id);
            var expectedNames = _listOfCreatedCapybaras.Select(f => f.Name).OrderBy(n => n).ToList();
            var actualNames = friendsList.Friends.Select(f => f.Name).OrderBy(n => n).ToList();
            ExtendedAssertions.AreCollectionsEqual(expectedNames, actualNames,
                "Friends list from API should match created friends");
        });
    }

    private async Task CreateCapybaraUsingUIAsync(CapybaraModel capybara)
    {
        var popup = await _capybarasPage.OpenCreateNewCapybaraPopupAsync();
        await popup.CreateNewCapybaraAsync(capybara);
        await _capybarasPage.SelectCapybaraViewAsync(capybara.Name);
    }
}