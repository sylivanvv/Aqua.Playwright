using Aqua.AppConfig.Configuration;
using Aqua.Framework.API;
using Aqua.Framework.Browser;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Aqua.TestRailIntegration.TestRailIntegration;

/// <summary>
///     This class is used for updating TestRail test cases steps & name using values from auto test body
/// </summary>
public class TestRailApiClient(IPlaywrightBrowserManager browserManager, IConfig config, ILogger<TestRailApiClient> log)
    : BasePlaywrightApiClient(browserManager, config, log)
{
    protected override string GetBaseUrl() => Config.TestRailConfig.Url;
    protected override Task<APIRequestNewContextOptions> GetContextOptionsAsync() =>
        Task.FromResult(new APIRequestNewContextOptions
        {
            BaseURL = GetBaseUrl(),
            HttpCredentials = new HttpCredentials
            {
                Username = Config.TestRailConfig.User,
                Password = Config.TestRailConfig.Password
            }
        });

    public async Task AddResultAsync(string runId, string caseId, int statusId, string comment)
    {
        if (!Config.TestRailConfig.Enabled) return;
        var payload = new { status_id = statusId, comment };
        await ExecuteRequestAsync("POST", $"index.php?/api/v2/add_result_for_case/{runId}/{caseId}", payload);
    }
}