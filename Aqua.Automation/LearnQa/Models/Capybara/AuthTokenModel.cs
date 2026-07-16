using System.Text.Json.Serialization;

namespace Aqua.Automation.LearnQa.Models.Capybara;

public record AuthTokenModel
{
    [property: JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}