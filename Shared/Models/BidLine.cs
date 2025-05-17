namespace Shared.Models;

public record BidLine(
    List<TargetingData> TargetingData,
    decimal BidFactor
);