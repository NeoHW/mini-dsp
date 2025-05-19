using System.Text.Json.Serialization;

namespace Shared.Models;

public class CampaignDetail
{
    public Guid CampaignId { get; init; }
    public decimal BudgetCap { get; private set; }
    public decimal BudgetSpent { get; private set; }
    public decimal RemainingBudget => Math.Max(BudgetCap - BudgetSpent, 0);
    public decimal BaseBid { get; private set; }
    public IReadOnlyCollection<BidLine> BidLines { get; }

    [JsonConstructor]
    public CampaignDetail(Guid campaignId, decimal budgetCap, decimal baseBid, IReadOnlyCollection<BidLine> bidLines)
    {
        if (budgetCap < 0)
            throw new ArgumentOutOfRangeException(nameof(budgetCap), "Budget cannot be negative.");
            
        if (baseBid < 0)
            throw new ArgumentOutOfRangeException(nameof(baseBid), "Base bid cannot be negative.");
            
        if (bidLines == null)
            throw new ArgumentNullException(nameof(bidLines));
            
        CampaignId = campaignId;
        BaseBid = baseBid;
        BudgetCap = budgetCap;
        BudgetSpent = 0;
        BidLines = new List<BidLine>(bidLines).AsReadOnly();
    }

    public void UpdateBudget(decimal newBudgetCap)
    {
        if (newBudgetCap < 0)
            throw new ArgumentOutOfRangeException(nameof(newBudgetCap), "Budget cannot be negative.");
        
        BudgetCap = newBudgetCap; 
    }

    public bool SpendBudget(decimal amount)
    {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Deduction amount must be positive.");
            
            if (amount > RemainingBudget)
                return false;
            
            BudgetSpent  += amount;
            return true;
    }

    public void RefundBudget(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Refund amount must be positive.");
        
        if (amount > BudgetSpent)
            throw new ArgumentOutOfRangeException(nameof(amount), "Refund amount cannot exceed spent budget.");
        
        BudgetSpent -= amount;
    }
}