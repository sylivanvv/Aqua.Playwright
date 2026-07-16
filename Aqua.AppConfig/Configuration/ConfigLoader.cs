using Aqua.AppConfig.ConfigurationModels.Envs;
using Microsoft.Extensions.Configuration;

namespace Aqua.AppConfig.Configuration;

public static class ConfigLoader
{
    public static IConfig Load()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("config.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables();
        var configurationRoot = builder.Build();
      
             var envSelected = configurationRoot.GetValue<string>("EnvSelected");
        if (string.IsNullOrWhiteSpace(envSelected))
            throw new InvalidOperationException("The 'EnvSelected' property is missing in configuration.");

        var envKey = envSelected.ToLowerInvariant().Trim();
        var activeEnvSection = configurationRoot.GetSection($"EnvUrls:{envKey}");

        if (!activeEnvSection.Exists())
            throw new KeyNotFoundException(
                $"Environment '{envKey}' specified in 'EnvSelected' was not found in 'EnvUrls' dictionary.");
        var activeEnv = activeEnvSection.Get<EnvironmentSettings>()!;
        var config = new Config { Env = activeEnv };
        configurationRoot.Bind(config);
        return config;
    }
}