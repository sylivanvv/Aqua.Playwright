using System.Text.Json;

namespace Aqua.DataReader.Files;

public static class FileReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<T> GetJsonAsync<T>(string pathToFile)
    {
        var finalPathToFile = Path.Combine(AppContext.BaseDirectory, pathToFile);
        if (!File.Exists(finalPathToFile))
            throw new FileNotFoundException($"Test data file not found: {finalPathToFile}");
        await using var fileStream = new FileStream(finalPathToFile, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true);
        var result = await JsonSerializer.DeserializeAsync<T>(fileStream, JsonOptions);
        return result ??
               throw new JsonException($"Failed to deserialize JSON from '{finalPathToFile}' or the file is empty.");
    }
}