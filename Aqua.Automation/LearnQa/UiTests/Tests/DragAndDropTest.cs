using Allure.Net.Commons;
using Allure.NUnit.Attributes;
using Aqua.Automation.LearnQa.PageObjects.Pages;
using Aqua.Automation.LearnQa.PageObjects.UrlCreator;
using Aqua.Automation.Utils;
using Aqua.Framework.Extensions;

namespace Aqua.Automation.LearnQa.UiTests.Tests;

[Parallelizable(ParallelScope.All)]
[AllureFeature("Drag and Drop Capabilities")]
public class DragAndDropTest : BaseLearnQaTest
{
    private DragAndDropPage _dragAndDropPage;

    [SetUp]
    [AllureBefore("Open Drag and Drop page")]
    public async Task SetUp() => _dragAndDropPage = await Page
        .OpenPageAsync<DragAndDropPage>(LearnQaUrlCreator.DragAndDropPage);

    [Test]
    [Category(TestCategories.LearnQa)]
    [AllureStory("User can drag items into the drop zone")]
    [AllureName("Verify dragged items appear in the drop zone correctly")]
    public async Task DragItemsInDropZone()
    {
        var expectedItems = await AllureApi.Step("Drag random items to the drop zone",
            async () => await _dragAndDropPage.DragRandomItemsToDragZoneAsync());
        var actualItems = await AllureApi.Step("Retrieve the names of the items currently in the drop zone",
            async () => await _dragAndDropPage.GetListOfDraggableElementsInZoneAsync());
        AllureApi.Step("Verify that dropped items exactly match the expected list",
            () => ExtendedAssertions.AreCollectionsEqual(expectedItems, actualItems));
    }
}