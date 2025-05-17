using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Models;

namespace Shared.Utilities;

public static class UserLoader
{
    public static List<UserData> LoadFromFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        
        var users = JsonSerializer.Deserialize<List<UserData>>(json, options);
        return users ?? new List<UserData>();
    }
}