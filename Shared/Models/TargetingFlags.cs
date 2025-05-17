namespace Shared.Models;

[Flags]
public enum TargetingFlags : ulong
{
    None                   = 0,
    Male                   = 1UL << 0,
    Female                 = 1UL << 1,
    HighIncomeCategory     = 1UL << 2,
    AgeBand25To35          = 1UL << 3,
    AgeBand35To50          = 1UL << 4,
    LivesInMetroArea       = 1UL << 5,
    LivesInRegionalArea    = 1UL << 6
}

public class TargetingExtensions
{
    public static TargetingFlags ToFlags(IEnumerable<TargetingData> list) =>
        list.Aggregate(TargetingFlags.None, (acc, t) => acc | t switch
        {
            TargetingData.Male => TargetingFlags.Male,
            TargetingData.Female => TargetingFlags.Female,
            TargetingData.HighIncomeCategory => TargetingFlags.HighIncomeCategory,
            TargetingData.AgeBand25To35 => TargetingFlags.AgeBand25To35,
            TargetingData.AgeBand35To50 => TargetingFlags.AgeBand35To50,
            TargetingData.LivesInMetroArea => TargetingFlags.LivesInMetroArea,
            TargetingData.LivesInRegionalArea => TargetingFlags.LivesInRegionalArea,
            _ => TargetingFlags.None
        }); 
}