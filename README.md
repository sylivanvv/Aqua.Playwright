![.NET](https://img.shields.io/badge/.NET-10-blueviolet)
![C#](https://img.shields.io/badge/C%23-14-blue)
![Playwright](https://img.shields.io/badge/Playwright-1.61-green)
![NUnit](https://img.shields.io/badge/NUnit-4-orange)
![DI](https://img.shields.io/badge/Dependency%20Injection-Built--In-blue)
![Allure](https://img.shields.io/badge/Reporting-Allure-red)
![License](https://img.shields.io/badge/License-MIT-yellow)

# Aqua.Playwright

A modern, async-first, enterprise-grade C# test automation framework built natively on Microsoft Playwright.<br>
While Playwright provides powerful browser automation out of the box, building scalable, stable, and parallel-ready test suites for large enterprise applications still requires a robust architecture. Aqua.Playwright bridges this gap by acting as an orchestration layer that handles isolated dependency scopes, auth generation once per run, custom component packaging, and automated failure diagnostics.

---

## Why Aqua.Playwright?
**One browser - many contexts** - Playwright launches a dedicated browser instance per test by default. Aqua.Playwright reuses a single browser process while maintaining isolation through independent browser contexts, significantly reducing startup overhead.

**Isolated Parallel Execution** - True thread-safety utilizing Microsoft Dependency Injection and AsyncLocal context propagation. Each parallel test executes within its own isolated service scope with dedicated browser contexts, preventing state leaks.

**Global Authentication Pre-Generation** - Eliminates redundant UI login steps. Authentication states (storage cookies and tokens) are generated once per run and injected dynamically into isolated parallel contexts.

**High-Level Page Components** - Complex UI structures like `Table<TTableRow>` and `Dropdown` are encapsulated into reusable, lazy-evaluated components featuring built-in smart waits and resilient interaction strategies.

**Seamless UI + API Integration** - A unified API client layer that shares the browser's session context and handles automatic request-response logging and serialization.

**Advanced Failure Diagnostics** - Automated collection of screenshots, browser console logs, and full Playwright Trace Viewer zip files, generated and attached to Allure on test failure.


---

## Key Features

### 1. One Browser Process, Isolated Contexts Per Test

By default, Playwright launches a dedicated browser instance for every test. While this guarantees isolation, it also introduces unnecessary startup overhead when running large test suites.
Aqua.Playwright reuses a single browser process for the entire test run and creates a lightweight, isolated IBrowserContext for each test. Every parallel worker receives:
- Its own browser context
- Its own page instance
- Its own DI scope
- Isolated cookies, local storage, and session state

This approach preserves complete test isolation while significantly reducing browser startup overhead.
**Global Browser Initialization**
The browser process is launched once during the global test setup and reused throughout the entire run.
``` csharp
// Startup.cs (Service registrations)
services.AddSingleton<IPlaywrightBrowserManager, PlaywrightBrowserManager>();

// GlobalSetup.cs [SetupFixture]
[OneTimeSetUp]
public async Task RunBeforeAnyTests()
{
    var browserManager = RootProvider.GetRequiredService<IPlaywrightBrowserManager>();
    await browserManager.InitializeAsync();
}
``` 
**Per-Test Context Creation**
Every test creates a fresh browser context and a dedicated dependency injection scope.

---

``` csharp
[SetUp]
public async Task SetUpAsync()
{
    // Creates a new isolated DI scope for the current parallel thread
    AquaServices.CreateTestScope(GlobalSetup.RootProvider);

    var browserManager = AquaServices.Get<IPlaywrightBrowserManager>();

    // Creates a clean, isolated environment (cookies, cache, etc.)
    _browserContext = await browserManager.CreateContextAsync();
    Page = await _browserContext.NewPageAsync();
}
``` 
**Automatic Cleanup**
When the test finishes, only the isolated browser context and scoped services are disposed. The shared browser process remains alive until the entire test run completes.
``` csharp
[TearDown]
public async Task TearDownAsync()
{
    try
    {
        if (_browserContext != null) 
            await _browserContext.DisposeAsync();
    }
    finally
    {
        // Safely dispose the test's DI scope and free memory
        await AquaServices.ClearScopeAsync();
    }
}
```
---

### 2. Component-Based Page Objects

Aqua.Playwright promotes reusable UI components instead of large Page Objects filled with raw locators. Complex controls such as tables and dropdowns are encapsulated behind strongly typed abstractions, keeping tests concise, maintainable, and easy to read.

Pages can define their own load condition by overriding `WaitForLoadedAsync()`. Navigation helpers automatically wait for the page
to become ready before returning it to the caller.

```csharp
public class ExamplePage(IPage page) : BasePage(page)
{
    public override async Task WaitForLoadedAsync() => await CreateNewButton.WaitForAsync();

    private ILocator CreateNewButton => Page.GetByRole(AriaRole.Button, new() { Name = "Create New" });

    // Encapsulate table logic into a strongly typed component
    public Table<ExampleRow> ExampleTable => new(
        Page.Locator("table.example-grid"),
        rowFactory: locator => new ExampleRow(locator),
        rowSelector: "tr.example-row"
    );

    // Advanced handler for custom dropdowns
    public Dropdown SortDropdown => new(
        Page.Locator("button[role='combobox']"),
        Page.Locator("div[role='option']"),
        searchLocator: Page.Locator("input[placeholder='Search option...']")
    );
}
```
Typed Table Row

Every row is represented by a dedicated component with locators scoped to its own boundaries. This makes interactions predictable and allows table data to be mapped directly into strongly typed models.

```csharp
public class ExampleRow(ILocator rowLocator) : BaseTableRow(rowLocator), IComparableRow<ExampleModel>
{
    // Locators are strictly scoped to this specific row, not the whole page
    public ILocator NameLabel => RowLocator.Locator(".name");
    public ILocator AgeLabel => RowLocator.Locator(".age");
    public ILocator ViewButton => RowLocator.GetByRole(AriaRole.Button, new() { Name = "View" });

    // Maps the UI row directly to a C# data model
    public async Task<ExampleModel> AsDataAsync() => new(
        Name: await NameLabel.InnerTextAsync(),
        Age: (await AgeLabel.InnerTextAsync()).ToInt()
    );    
}

public record ExampleModel(string Name, int Age);
```
**Usage - Interact with complex controls:**

Dropdown interactions:

```csharp
// Select random option
await _page.SortDropdown.SelectRandomOptionAsync();

// Select a specific option (if the dropdown has a search locator, it will filter by this query first)
await _page.SortDropdown.SelectOptionAsync("Specific option");
```

Table interactions:
```csharp
// Find a specific row and use its scoped elements
var row = await _page.ExampleTable.GetRowAsync("John Doe");
await row.ViewButton.ClickAsync();

// Convert the entire table into strongly typed models
List<ExampleModel> allRows = await _page.ExampleTable.GetAllDataAsync<ExampleModel>();

// Select random table rows and get their data models
var randomRows = await _page.ExampleTable.GetRandomRowsAsync();
List<ExampleModel> randomModels = await randomRows.GetAllDataAsync();
```
---
### 3. Shared Authentication & Session Management
Authenticating through the UI before every test is one of the largest sources of execution time and flakiness in browser automation.
Aqua.Playwright generates authentication states once per test run, caches and injects them automatically into isolated browser contexts. The same session can also be reused by API clients, enabling seamless hybrid UI + API testing without manually synchronizing cookies or tokens.
**Declarative Authentication**

Authentication is requested through attributes at either the fixture level.

```csharp
[TestFixture]
[Parallelizable(ParallelScope.All)]
[WithAuth(AuthRole.StandardUser)]
public class ExampleTests : BasePlaywrightTest
{
    [Test]
    public async Task UserCanAccessDashboard()
    {
        var page = await Page.OpenPageAsync<DashboardPage>();
        Assert.That(await page.IsDisplayedAsync());
    }
}
```

**Authentication State Generation**

Before the test run starts, authentication states are generated once and stored as Playwright `storageState` files.

```csharp
[OneTimeSetUp]
public async Task RunBeforeAnyTests()
{
    await authStateGenerator.GenerateAllAsync();
}
```

**Automatic Session Injection**

When a test starts, the framework loads the requested authentication state and injects it directly into the new browser context.

```csharp
private BrowserNewContextOptions GetBrowserContextOptions()
{
    return new()
    {
        StorageStatePath = GetStorageStatePath()
    };
}
```

**Shared API Context**

API clients can bind directly to the active browser session.

```csharp
Client.SetTraceContext(Page.APIRequest);
```

Once bound, API requests automatically inherit:

* authentication cookies
* local storage state
* session tokens
* request context configuration

Benefits:
* Login is performed once per test run
* Browser contexts start already authenticated
* API clients automatically reuse the active browser session
* No manual cookie or token synchronization
* UI and API actions appear in the same Playwright Trace Viewer timeline

---
### 4. Automated Failure Diagnostics

Screenshots, browser errors, and Playwright traces are collected automatically and attached to Allure reports on failure.

Debugging UI tests on CI/CD is difficult when the only available information is a failed assertion. Aqua.Playwright captures diagnostic artifacts automatically and makes them available directly from the test report.

The framework follows a "record always, save on failure" strategy:

Playwright tracing runs silently during test execution
screenshots are captured automatically on failure
browser console errors and unhandled JavaScript exceptions are collected
diagnostics are attached to Allure without any test-side code
```csharp
[TearDown]
public virtual async Task TearDownAsync()
{
    var isFailed =
        TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed;

    try
    {
        await ReportBrowserErrorsAsync();
        await TryTakeScreenshotOnFailAsync(isFailed);
        await TrySavePlaywrightTraceOnFailAsync(isFailed);
    }
    finally
    {
        await AquaServices.ClearScopeAsync();
    }
}
```

**Captured Artifacts**

| Artifact | Description |
|---|---|
| Allure Attachments | Automatic report integration with info about test run |
| Playwright Trace | Full execution timeline with DOM snapshots, network traffic and Playwright actions |
| Screenshot | Browser viewport at the moment of failure |
| Log file | Text file with info logged by NLog |
| Console Errors | Browser console.error or server error messages |
| JS Exceptions | Unhandled frontend runtime errors |


## Test Example:

```csharp
[Parallelizable(ParallelScope.All)]
[AllureFeature("Example Features")]
[WithAuth(AuthRole.StandardUser)]
public class ExampleTest : BasePlaywrightTest
{
    private ExamplePage _examplePage;
    private UserModel? _createdUser;

    protected UserApiClient Client => field ??= AquaServices.Get<UserApiClient>();
    
    [SetUp]
    [AllureBefore("Navigate to Example page and set UI context to API Client")]
    public async Task SetUp() 
    {
        _examplePage = await Page.OpenPageAsync<ExamplePage>(UrlCreator.ExamplePage);
        Client.SetTraceContext(Page.APIRequest);
    }
    
    [TearDown]
    [AllureAfter("Cleanup: Delete created data")]
    public async Task DeleteCreatedUserAsync()
    {
        if (_createdUser is not null)
            await Client.DeleteUser(_createdUser.Email);
    }
    [Test]
    [Category(TestCategories.Regression)]
    [AllureName("Create User via API and verify its data on UI")]
    public async Task CreateUserUsingAPIAndVerifyItsCreatedOnUI()
    {
        var newUser = UserModel.CreateRandom();
        await AllureApi.Step("1. Arrange: Create User via API POST request", async () =>
            _createdUser = await Client.CreateUser(newUser));
        await AllureApi.Step("2. Act: Refresh UI to fetch new data", async () => 
        {
            _examplePage = await Page.ReloadAsync<ExamplePage>();
            await _examplePage.SelectUserViewAsync(newUser.FirstName);
 		});
        await AllureApi.Step("3. Assert: Verify UI displays correct data", async () =>
        {
            var uiRow = await _examplePage.UsersTable.GetRowAsync(newUser.Email);
            var uiModel = await uiRow.AsDataAsync();
            ExtendedAssertions.AreEqual(_createdUser, uiModel, "Info from api and ui should be equal");
        });
    }
}
```
---

## Running Tests
<details>
<summary>Show setup & execution guide</summary>

    
### Requirements

Before running the tests, ensure the following software is installed:

- .NET 10 SDK
- Google Chrome
- Allure CLI (optional, for generating reports)

### 1. Clone the Repository

```bash
git clone https://github.com/sylivanvv/Aqua.Playwright.git
cd Aqua.Automation
```

### 2. Donwload browser binaries
```bash
dotnet build
pwsh bin/Debug/netX.0/playwright.ps1 install
```

### 3. Configure the Target Environment

Aqua uses a multi-layered configuration approach. By default, environments (QA, Dev, Staging) are mapped inside the config.json file.
You can either modify the config.json file directly:

```json
{
  "envSelected": "qa",
  "envUrls": {
    "qa": {
      "exampleUrl": "https://qa.example.com",
      "exampleUrlApiUrl": "https://api.qa.example.com",
      "dbConnectionString": "data source = ..."
    },    
    "dev1": {
      "exampleUrl": "https://dev1.example.com",
      "exampleUrlApiUrl": "https://api.dev1.example.com",
      "dbConnectionString": "data source = ..."
    }
  }
}
```

Or override any setting on the fly using standard .NET environment variables (using `__` as the hierarchy separator) when running tests. This makes the framework CI/CD ready out of the box:

```bash
envSelected=dev1 dotnet test
```

Override any nested configuration value:

```bash
TimeoutSettings__ExplicitSeconds=5 dotnet test
```

### 4. Run the Tests

Run everything:

```bash
dotnet test
```

Run a specific category:

```bash
dotnet test --filter "Category=CategoryName"
```

### 5. Generate an Allure Report

During the test execution, Aqua automatically collects Playwright Traces, logs, screenshots, and API attachments, saving them to the allure-results folder.
To build and open the interactive HTML report in your browser, run:

```bash
allure serve allure-results
```

### 6. Diagnose failed tests with Playwright Trace

If a test fails, a full Playwright Trace (containing DOM snapshots, network requests, and console logs) is saved as a zip file. To open the interactive Trace Viewer, run:

```bash
pwsh bin/Debug/netX.0/playwright.ps1 show-trace bin/Debug/netX.0/playwright-traces/PlaywrightTests.ExampleTest.TestName.zip
```
</details>

---

## Tech Stack

| Technology | Purpose |
|---|---|
| C# 14 / .NET 10 | Language and runtime |
| Microsoft Playwright | Browser automation & API client |
| NUnit 4 | Test runner |
| Microsoft.Extensions.DependencyInjection | IoC container |
| Allure.NUnit | Test reporting |
| NLog | Logging |
| Dapper + MiniExcel | DB and Excel test helpers |

---

## Allure Report

![Allure Report](docs/allure-report.png)

---

## Project Structure

```text
Aqua.Automation/
├── Aqua.AppConfig/                # Configuration loading
├── Aqua.Automation/               # Actual NUnit test suites, Page Objects, and test data models
├── Aqua.DataReader/               # DB and Excel test data helpers
├── Aqua.Framework/                # Core Engine: DI setup, PlaywrightApiClient and PlaywrightBrowserManager, Pages, and UI Components
└── Aqua.TestRailIntegration/      # TestRail API client for results reporting
```
---

To demonstrate the framework's capabilities, a comprehensive suite of automated tests was built using the following training sandboxes: [LearnQA](https://www.learnaqa.info/) and [QaBrains](https://practice.qabrains.com/ecommerce).

---
## Author

Built by [Vladimir Silivanov](https://github.com/sylivanvv) as a portfolio project
demonstrating test automation architecture in C#.

Feel free to reach out on [LinkedIn](https://www.linkedin.com/in/vsilivanov/).<br>
Email - v.salivan13@gmail.com

Other projects: <br>
- [Aqua.Automation (Selenium c#)](https://github.com/sylivanvv/Aqua.Playwright)<br>
- [Aqua.Appium.TS](https://github.com/sylivanvv/Aqua.Appium.TS)
