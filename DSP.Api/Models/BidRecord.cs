namespace DSP.Api.Models;

public class BidRecord
{
    public Guid BidId { get; set; }
    
    public Guid CampaignId { get; set; }
    
    public decimal BidAmount { get; set; }

    public bool? IsWinner { get; set; }
}