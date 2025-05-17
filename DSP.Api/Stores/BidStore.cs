using System.Collections.Concurrent;
using DSP.Api.Models;
using Shared.Models;

namespace DSP.Api.Stores;


public interface IBidStore
{
   BidRecord AddBid(BidRecord bidRecord);
   BidRecord GetBidById(Guid bidId);
   bool AddFeedbackResult(BidFeedback feedback);
   IReadOnlyCollection<BidRecord> GetBidsByCampaign(Guid campaignId);
}


public class BidStore : IBidStore
{
   private readonly ConcurrentDictionary<Guid, BidRecord> _bids = new();

   public BidRecord AddBid(BidRecord bid)
   {
      if (bid == null)
         throw new ArgumentNullException(nameof(bid));
            
      return _bids.AddOrUpdate(
         bid.BidId,
         bid,
         (_, _) => bid
      );
   }
   
   public BidRecord? GetBidById(Guid bidId)
   {
      return _bids.GetValueOrDefault(bidId);
   }
   
   public bool AddFeedbackResult(BidFeedback feedback)
   {
      if (feedback == null)
         throw new ArgumentNullException(nameof(feedback));
            
        
      if (!_bids.TryGetValue(feedback.BidId, out var bid))
      {
         return false;
      }
      
      bid.IsWinner = feedback.Win;
      
      return true;
   } 
   
   public IReadOnlyCollection<BidRecord> GetBidsByCampaign(Guid campaignId)
   {
      return _bids.Values
         .Where(b => b.CampaignId == campaignId)
         .ToList()
         .AsReadOnly();
   }
}