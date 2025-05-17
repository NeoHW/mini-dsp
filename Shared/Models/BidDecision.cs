namespace Shared.Models;

public record BidDecision(
    Guid   BidId,
    String DspName,
    Decimal BidAmount);