namespace Shared.Models;

public record UserData(
    string UserId,
    List<TargetingData> TargetingData);

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