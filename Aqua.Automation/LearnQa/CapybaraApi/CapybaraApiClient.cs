using System.Text.Json;
using Aqua.AppConfig.Configuration;
using Aqua.Automation.LearnQa.Models.Capybara;
using Aqua.Automation.LearnQa.PageObjects.UrlCreator;
using Aqua.Framework.API;
using Aqua.Framework.Browser;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

namespace Aqua.Automation.LearnQa.CapybaraApi;

public class CapybaraApiClient(IPlaywrightBrowserManager browserManager, IConfig config, ILogger<CapybaraApiClient> log)
    : BasePlaywrightApiClient(browserManager, config, log), IApiClient
{
    private string? _cachedToken;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    protected override string GetBaseUrl() => Config.Env.LearnQaApiUrl;

    protected override Task<APIRequestNewContextOptions> GetContextOptionsAsync() =>
        Task.FromResult(new APIRequestNewContextOptions
        {
            BaseURL = GetBaseUrl(),
        });

    protected override async Task ConfigureRequestAsync(APIRequestContextOptions options)
    {
        var token = await FetchTokenAsync(GetBaseUrl(), Config.AuthData.LearnQaUser.UserName,
            Config.AuthData.LearnQaUser.Password);
        options.Headers = new Dictionary<string, string>
        {
            { "Authorization", token },
            { "Accept", "application/json" }
        };
    }

    public Task<CapybaraModel> CreateCapybara(CapybaraModel capybara) =>
        PostAsync<CapybaraModel>("api/capybaras", capybara);

    public Task<FriendshipResponseDto> AddFriend(string capybaraId, string friendId) =>
        PostAsync<FriendshipResponseDto>($"api/capybaras/{capybaraId}/friends",
            new
            {
                friend_id = friendId
            });

    public Task<FriendsListDto> GetFriends(string capybaraId) =>
        GetAsync<FriendsListDto>($"api/capybaras/{capybaraId}/friends");

    public Task<CapybaraModel> UpdateCapybara(CapybaraModel capybara) =>
        PatchAsync<CapybaraModel>($"api/capybaras/{capybara.Id}", capybara);

    public Task DeleteCapybara(string capybaraId) => DeleteAsync($"api/capybaras/{capybaraId}");
    
    public async Task<CapybaraModel> GetCreatedCapybaraFromAPIAsync(string capybaraName)
    {
        var capybarasList = await GetAsync<CapybaraResponse>(LearnQaUrlCreator.CapybarasApi.SortedCapybarasEndpoint);
        return capybarasList.Capybaras.Find(c => string.Equals(c.Name, capybaraName)) ??
               throw new InvalidOperationException($"There is no capybara with name {capybaraName}");
    }
    
    private async Task<string> FetchTokenAsync(string baseUrl, string email, string password)
    {
        if (_cachedToken != null) return _cachedToken;
        await _tokenLock.WaitAsync();
        try
        {
            if (_cachedToken != null) return _cachedToken;
            await using var tempContext =
                await Playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions { BaseURL = baseUrl });
            var response = await tempContext.PostAsync("api/auth/login",
                new APIRequestContextOptions { DataObject = new { email, password } });
            if (!response.Ok)
            {
                var errorText = await response.TextAsync();
                throw new HttpRequestException($"Auth failed! Status: {response.Status}. Response: {errorText}");
            }

            var jsonBytes = await response.BodyAsync();
            var authModel = JsonSerializer.Deserialize<AuthTokenModel>(jsonBytes, JsonOptions);
            _cachedToken = $"Bearer {authModel!.AccessToken}";
            return _cachedToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }
}