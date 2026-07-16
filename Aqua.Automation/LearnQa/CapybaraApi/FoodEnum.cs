using System.Text.Json.Serialization;

namespace Aqua.Automation.LearnQa.CapybaraApi;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FoodEnum
{
    Orange,
    Watermelon,
    Lettuce,
    Carrot,
    Banana,
    Grass
}