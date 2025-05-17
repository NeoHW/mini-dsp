using System.Text.Json.Serialization;

namespace Shared.Models;

public record UserData(
    string UserId,
    List<TargetingData> TargetingData);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TargetingData
{
    Male,
    Female,
    HighIncomeCategory,
    AgeBand25To35,
    AgeBand35To50,
    LivesInMetroArea,
    LivesInRegionalArea
}