using System.Text.Json.Serialization;

namespace Aqua.Automation.LearnQa.CapybaraApi;

public record FriendsListDto
{
    [property: JsonPropertyName("capybara_id")]
    public string CapybaraId {  get; set; }

    [property: JsonPropertyName("friends")]
    public List<FriendDto> Friends { get; set; }

    [property: JsonPropertyName("total")]
    public int Total { get; set; }
}

public record FriendDto
{
    [property: JsonPropertyName("id")]
    public string Id { get; set; }

    [property: JsonPropertyName("name")]
    public string Name { get; set; }

    [property: JsonPropertyName("mood")]
    public string Mood { get; set; }
}