using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Models;

namespace Shared.Utilities;

public static class CampaignLoader
{
    public static List<CampaignDetail> LoadFromFile(string path)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        var json = File.ReadAllText(path);
        var rawCampaigns = JsonSerializer.Deserialize<List<CampaignDetail>>(json, options);

        if (rawCampaigns == null)
        {
            throw new InvalidDataException("Failed to deserialize campaigns from file.");
        }

        return rawCampaigns;
    }
}
