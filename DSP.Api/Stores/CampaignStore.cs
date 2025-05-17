using System.Collections.Concurrent;
using DSP.Api.Models;

namespace DSP.Api.Stores;
using Shared.Models;

public interface ICampaignStore
{
   CampaignDetail? GetCampaignById(Guid id);
   CampaignDetail AddCampaign(CampaignDetail campaignDetail);
   bool UpdateBudget(Guid id, decimal newBudget);
   IReadOnlyCollection<CampaignDetail> GetAll();
}

public class CampaignStore : ICampaignStore 
{
   private ConcurrentDictionary<Guid, CampaignDetail> _campaigns = new();

   public CampaignDetail AddCampaign(CampaignDetail campaignDetail)
   {
      return _campaigns.AddOrUpdate(
         campaignDetail.CampaignId,
         campaignDetail,
         (_, _) => campaignDetail
      );
   }
   
   public bool UpdateBudget(Guid id, decimal newBudget)
   {
      if (!_campaigns.TryGetValue(id, out var campaign)) return false;
      campaign.UpdateBudget(newBudget);
      return true;
   }
   
   public IReadOnlyCollection<CampaignDetail> GetAll()
   {
      return _campaigns.Values.ToList().AsReadOnly();
   }
   
   public CampaignDetail? GetCampaignById(Guid id) => _campaigns.GetValueOrDefault(id);
}