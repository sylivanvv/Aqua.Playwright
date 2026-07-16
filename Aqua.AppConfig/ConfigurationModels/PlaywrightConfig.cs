namespace Aqua.AppConfig.ConfigurationModels;

public record PlaywrightConfig
{
    public string BrowserName  { get; init; } = null!;
    
    public bool Headless  { get; init; }
    
    public int ElementWaitMs  { get; init; }
    
    public int PageLoadMs  { get; init; }
    
    public int ApiTimeoutMs  { get; init; }
}