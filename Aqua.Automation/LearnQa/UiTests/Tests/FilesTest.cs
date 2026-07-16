using System.Globalization;
using Allure.Net.Commons;
using Allure.NUnit.Attributes;
using Aqua.Automation.AuthHelpers;
using Aqua.Automation.LearnQa.Models.Excel;
using Aqua.Automation.LearnQa.PageObjects.Pages;
using Aqua.Automation.LearnQa.PageObjects.UrlCreator;
using Aqua.Automation.Utils;
using Aqua.DataReader.Excel;
using Aqua.Framework.Extensions;
using Aqua.Framework.Utils;
using Microsoft.Playwright;

namespace Aqua.Automation.LearnQa.UiTests.Tests;

[Parallelizable(ParallelScope.All)]
[WithAuth(AuthRole.LearnQaUser)]
public class FilesTest : BaseLearnQaTest
{
    private FileLoadingPage _filesPage;

    private const string FileName = "template_data.xlsx";
    private const string ProcessedFileName = "processed_template_data.xlsx";
    private static string FullPath => field ??= Path.Combine(PathProvider.DownloadsPath, FileName);
    private static string FullPathProcessed => field ??= Path.Combine(PathProvider.DownloadsPath, ProcessedFileName);

    [SetUp]
    [AllureBefore("Clean files and open FileLoadingPage")]
    public async Task SetUp()
    {
        File.Delete(FullPath);
        File.Delete(FullPathProcessed);
        _filesPage = await Page.OpenPageAsync<FileLoadingPage>(LearnQaUrlCreator.FileOperationsPage);
    }

    [OneTimeTearDown]
    [AllureAfter("Cleanup downloaded files")]
    public static void ClearFile()
    {
        File.Delete(FullPath);
        File.Delete(FullPathProcessed);
    }

    [Test]
    [Category(TestCategories.LearnQa)]
    [AllureStory("Excel Processing and File Download/Upload")]
    [AllureName("Download template, edit file data, upload, and verify processed results")]
    public async Task ExcelFileProcessing()
    {
        await AllureApi.Step("1. Arrange: Download template file", async () =>
        {
            var download = await _filesPage.DownloadFileAsync();
            await download.SaveAsAsync(FullPath);
            return download;
        });
        var expectedRows = await AllureApi.Step("2. Act: Modify template file and upload it back", async () =>
        {
            var rows = await ExcelReader.ReadDataAsync<EmployeeModel>(FullPath);
            RandomizeModel(rows);
            await ExcelReader.SaveDataAsync(FullPath, rows);
            ProcessModel(rows);
            await _filesPage.UploadFileAsync(FullPath);
            return rows;
        });
        await AllureApi.Step("3. Assert: Verify downloaded processed data matches system logic", async () =>
        {
            await Assertions.Expect(_filesPage.IsFileUploaded(FileName)).ToBeVisibleAsync();
            var download = await _filesPage.DownloadProcessedFileAsync();
            await download.SaveAsAsync(FullPathProcessed);
            var actualProcessedRows = ExcelReader.ReadData<EmployeeModel>(FullPathProcessed);
            ExtendedAssertions.AreCollectionsEqual(expectedRows, actualProcessedRows);
        });
    }

    private static void RandomizeModel(List<EmployeeModel> model)
    {
        foreach (var item in model)
        {
            item.Name = RandomGenerator.RandomString(5);
            item.Score /= 1.1m;
        }
    }

    private static void ProcessModel(List<EmployeeModel> model)
    {
        foreach (var item in model)
        {
            item.Name = item.Name.ToUpper();
            item.Score *= 1.1m;
            var date = DateTime.ParseExact(item.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            item.Date = date.ToString("dd/MM/yyyy");
        }
    }
}