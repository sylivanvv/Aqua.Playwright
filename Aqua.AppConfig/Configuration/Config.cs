using Aqua.AppConfig.ConfigurationModels;
using Aqua.AppConfig.ConfigurationModels.AuthData;
using Aqua.AppConfig.ConfigurationModels.Envs;

namespace Aqua.AppConfig.Configuration;

public sealed class Config : IConfig
{
    public EnvironmentSettings Env { get; init; } = null!;
    public string EnvSelected { get; init; } = null!;
    public string LoginCookieStoragePathLearnQa { get; init; } = null!;
    public string LoginCookieStoragePathQaBrains { get; init; } = null!;
    public AuthData AuthData { get; init; } = null!;
    public TestRailConfig TestRailConfig { get; init; } = null!;
    public PlaywrightConfig PlaywrightConfig { get; init; } = null!;
}