namespace Shared.Models;

public record BidFeedback(
    Guid BidId,
    bool Win);