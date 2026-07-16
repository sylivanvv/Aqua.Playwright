namespace Aqua.AppConfig.ConfigurationModels;

public record TestRailConfig
{
    public bool Enabled { get; init; }
    public string Url { get; init; } = null!;

    public string User { get; init; } = null!;

    public string Password { get; init; } = null!;

    public string RunId { get; init; } = null!;

    public bool Update { get; init; }
}
