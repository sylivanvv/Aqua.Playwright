using Aqua.AppConfig.ConfigurationModels;
using Aqua.AppConfig.ConfigurationModels.AuthData;
using Aqua.AppConfig.ConfigurationModels.Envs;

namespace Aqua.AppConfig.Configuration;

public interface IConfig
{
    EnvironmentSettings Env { get; init; }
    AuthData AuthData { get; init; }
    string LoginCookieStoragePathLearnQa  { get; init; }
    string LoginCookieStoragePathQaBrains  { get; init; }
    TestRailConfig TestRailConfig { get; init; }
    PlaywrightConfig PlaywrightConfig { get; init; }
}