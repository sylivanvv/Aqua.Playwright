using System.Text.Json.Serialization;
using Aqua.Automation.LearnQa.CapybaraApi;
using Aqua.Framework.Utils;

namespace Aqua.Automation.LearnQa.Models.Capybara;

public record CapybaraModel
{
    [property: JsonPropertyName("name")]
    public string Name { get; set; }

    [property: JsonPropertyName("age")]
    public int Age { get; set; }

    [property: JsonPropertyName("bio")]
    public string Bio { get; set; }

    public string? Id { get; set; }

    [property: JsonPropertyName("favorite_food")]
    public FoodEnum FavoriteFood { get; set; }

    public static CapybaraModel CreateRandom() => new()
    {
        Name = GenerateRandomName(),
        Age = GenerateRandomAge(),
        Bio = GenerateRandomBio(),
        FavoriteFood = GetRandomFood()
    };
    
    private static string GenerateRandomName() => RandomGenerator.GenerateStringWithLabel();
    private static int GenerateRandomAge() => RandomGenerator.GetRandomInt(1, 15);
    private static string GenerateRandomBio() => $"Bio {RandomGenerator.RandomString(10)}";

    private static FoodEnum GetRandomFood()
    {
        var values = Enum.GetValues<FoodEnum>();
        return values[Random.Shared.Next(values.Length)];
    }
}