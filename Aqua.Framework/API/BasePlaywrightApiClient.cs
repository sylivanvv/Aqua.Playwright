using System.Text.Json;
using System.Text.Json.Serialization;
using Aqua.AppConfig.Configuration;
using Aqua.Framework.Browser;
using Aqua.Framework.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Aqua.Framework.API;

/// <summary>
/// An abstract base class for Playwright-based API clients.
/// Provides thread-safe lazy initialization, automatic JSON serialization/deserialization, 
/// centralized error handling, and seamless integration with Playwright Trace Viewer.
/// Inherit from this class to implement a typed API client for a specific service.
/// Override <see cref="GetBaseUrl"/> and <see cref="GetContextOptionsAsync"/>
/// to configure the target endpoint and authentication.
/// Optionally override <see cref="ConfigureRequestAsync"/> to inject per-request headers or other options.
/// </summary>
public abstract class BasePlaywrightApiClient(IPlaywrightBrowserManager browserManager, IConfig config, ILogger log)
    : IAsyncDisposable
{
    protected readonly IConfig Config = config;
    protected IPlaywright Playwright => field ??= browserManager.PlaywrightInstance;

    /// <summary>
    /// The Playwright API request context used for standalone (non-trace) execution.
    /// Initialized lazily on the first request via <see cref="EnsureContextAsync"/>.
    /// </summary>
    private IAPIRequestContext? _context;

    /// <summary>
    /// Optional browser page API context set via <see cref="SetTraceContext"/>.
    /// When present, all requests are routed through this context so they appear in Playwright traces.
    /// Takes priority over <see cref="_context"/>.
    /// </summary>
    private IAPIRequestContext? _uiContext;

    /// <summary>
    /// Prevents race conditions during the lazy initialization of the API context.
    /// </summary>
    private readonly SemaphoreSlim _contextLock = new(1, 1);

    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Returns the base URL for all relative endpoint paths used by this client.
    /// Must be implemented by each concrete API client (e.g. <c>"https://api.example.com"</c>).
    /// </summary>
    protected abstract string GetBaseUrl();

    /// <summary>
    /// Builds and returns the Playwright API request context options for this client,
    /// including base URL, default headers, authentication tokens, etc.
    /// Called once during lazy context initialization.
    /// </summary>
    protected abstract Task<APIRequestNewContextOptions> GetContextOptionsAsync();

    /// <summary>
    /// A hook method allowing child classes to dynamically modify request options
    /// </summary>
    protected virtual Task ConfigureRequestAsync(APIRequestContextOptions options) => Task.CompletedTask;

    protected virtual float? RequestTimeoutMs => Config.PlaywrightConfig.ApiTimeoutMs;

    /// <summary>
    /// Returns the existing <see cref="_context"/> if already initialized,
    /// or creates a new one using a double-checked locking pattern to ensure
    /// thread safety under parallel test execution.
    /// </summary>
    private async Task<IAPIRequestContext> EnsureContextAsync()
    {
        if (_context != null) return _context;

        await _contextLock.WaitAsync();
        try
        {
            if (_context != null) return _context;

            var options = await GetContextOptionsAsync();
            _context = await Playwright.APIRequest.NewContextAsync(options);
            return _context;
        }
        finally
        {
            _contextLock.Release();
        }
    }

    /// <summary>
    /// Binds the API client to an existing browser page's request context.
    /// Use this in hybrid tests to ensure API calls are captured in the Playwright Trace archive.
    /// </summary>
    /// <param name="uiContext">The APIRequestContext from the active Playwright Page.</param>
    public void SetTraceContext(IAPIRequestContext uiContext) => _uiContext = uiContext;

    /// <summary>
    /// Executes an HTTP request using Playwright's fetch API.
    /// Serializes the request body to JSON if provided, applies per-request configuration,
    /// logs the request and response.
    /// </summary>
    /// <param name="method">HTTP method (e.g. <c>"GET"</c>, <c>"POST"</c>, <c>"DELETE"</c>).</param>
    /// <param name="url">Relative or absolute endpoint URL.</param>
    /// <param name="data">Optional request body. Will be serialized to JSON using <see cref="JsonOptions"/>.</param>
    /// <returns>The Playwright <see cref="IAPIResponse"/>.</returns>
    protected async Task<IAPIResponse> ExecuteRequestAsync(string method, string url, object? data = null)
    {
        var context = _uiContext ?? await EnsureContextAsync();
        var jsonPayload = data != null ? JsonSerializer.Serialize(data, JsonOptions) : null;
        if (data != null) log.Info("Payload: {json}", jsonPayload!);
        var requestOptions = new APIRequestContextOptions
        {
            Method = method,
            Data = jsonPayload,
            Timeout = RequestTimeoutMs
        };
        await ConfigureRequestAsync(requestOptions);
        if (data != null)
        {
            var headers =
                requestOptions.Headers?.ToDictionary(k =>
                    k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            headers.TryAdd("Content-Type", "application/json");
            requestOptions.Headers = headers;
        }

        var fullUrl = BuildAbsoluteUrl(url);
        log.Info("REQUEST: {method} {fullUrl}", method, fullUrl);
        var response = await context.FetchAsync(fullUrl, requestOptions);

        log.Info("RESPONSE: {responseStatus}", response.Status);
        return response;
    }

    /// <summary>
    /// Safely concatenates the base URL and the endpoint, preventing double-slash formatting issues.
    /// </summary>
    private string BuildAbsoluteUrl(string endpoint)
    {
        if (endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return endpoint;
        }

        var baseUrl = GetBaseUrl();
        return string.IsNullOrEmpty(baseUrl) ? endpoint : $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
    }

    /// <summary>
    /// Executes the HTTP request and deserializes the JSON response body into the specified DTO model.
    /// </summary>
    /// <typeparam name="T">The expected response model type.</typeparam>
    /// <param name="method">HTTP method.</param>
    /// <param name="url">Relative or absolute endpoint URL.</param>
    /// <param name="data">Optional request body.</param>
    /// <returns>The deserialized response object.</returns>
    /// <exception cref="InvalidOperationException">Thrown if deserialization returns null.</exception>
    protected async Task<T> SendAndDeserializeAsync<T>(string method, string url, object? data = null)
    {
        await using var response = await ExecuteRequestAsync(method, url, data);
        if (!response.Ok)
        {
            var errorBody = await response.TextAsync();
            throw new HttpRequestException(
                $"API request failed: {method} {url}. Status: {response.Status}. Body: {errorBody}");
        }

        var jsonBytes = await response.BodyAsync();
        var result = JsonSerializer.Deserialize<T>(jsonBytes, JsonOptions);
        return result ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public Task<T> GetAsync<T>(string url) =>
        SendAndDeserializeAsync<T>("GET", url);

    public Task<T> PostAsync<T>(string url, object? data = null) =>
        SendAndDeserializeAsync<T>("POST", url, data);

    public Task<T> PatchAsync<T>(string url, object? data = null) =>
        SendAndDeserializeAsync<T>("PATCH", url, data);

    public async Task DeleteAsync(string url)
    {
        await using var response = await ExecuteRequestAsync("DELETE", url);
        if (!response.Ok)
        {
            var errorBody = await response.TextAsync();
            throw new HttpRequestException(
                $"API request failed: DELETE {url}. Status: {response.Status}. Body: {errorBody}");
        }
    }

    /// <summary>
    /// Asynchronously releases all resources associated with the API context and locks.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_context != null) await _context.DisposeAsync();
        _contextLock.Dispose();
        GC.SuppressFinalize(this);
    }
}