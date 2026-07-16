namespace Aqua.AppConfig.ConfigurationModels.Envs;


public record EnvironmentSettings
{
    public string QaBrainsBaseUrl { get; init; } = null!;
    public string LearnQaBaseUrl { get; init; } = null!;
    public string LearnQaApiUrl { get; init; } = null!;
    public string DbConnectionString { get; init; } = null!;
}