using System.Text.Json.Serialization;

namespace Shared.Models;

public class CampaignDetail
{
    public Guid CampaignId { get; init; }
    public decimal Budget { get; private set; }
    public decimal RemainingBudget { get; private set; }
    public decimal BaseBid { get; private set; }
    public IReadOnlyCollection<BidLine> BidLines { get; }

    [JsonConstructor]
    public CampaignDetail(Guid campaignId, decimal budget, decimal remainingBudget, decimal baseBid, IReadOnlyCollection<BidLine> bidLines)
    {
        if (budget < 0)
            throw new ArgumentOutOfRangeException(nameof(budget), "Budget cannot be negative.");
            
        if (baseBid < 0)
            throw new ArgumentOutOfRangeException(nameof(baseBid), "Base bid cannot be negative.");
            
        if (bidLines == null)
            throw new ArgumentNullException(nameof(bidLines));
            
        CampaignId = campaignId;
        BaseBid = baseBid;
        Budget = budget;
        RemainingBudget = remainingBudget;
        BidLines = new List<BidLine>(bidLines).AsReadOnly();
    }

    public void UpdateBudget(decimal newBudget)
    {
        if (newBudget < 0)
            throw new ArgumentOutOfRangeException(nameof(newBudget), "Budget cannot be negative.");
        
        RemainingBudget = newBudget; 
    }

    public bool DeductFromRemainingBudget(decimal amount)
    {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Deduction amount must be positive.");
            
            if (amount > RemainingBudget)
                return false;
            
            RemainingBudget -= amount;
            return true;
    }

    public void AddToRemainingBudget(decimal amount)
    {
        RemainingBudget += amount;
    }
}