namespace Aqua.AppConfig.ConfigurationModels.AuthData;

public record AuthData
{
    public Credentials LearnQaUser { get; init; } = null!;
    public Credentials QaBrainsUser { get; init; } = null!;
}

public record Credentials
{
    public string UserName { get; init; } = null!;
    public string Password { get; init; } = null!;
}