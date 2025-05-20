using System.Diagnostics;
using Shared.Models;
using DSP.Api.Models;
using DSP.Api.Stores;
using SSP.Api;

namespace DSP.Api;

public class Dsp
{
    private readonly string _name;
    private readonly ICampaignStore _campaignStore;
    private readonly IUserStore _userStore;
    private readonly IBidStore _bidStore;

    public Dsp(string name, IUserStore userStore, List<CampaignDetail> campaigns)
    {
        _name = name;
        _userStore = userStore;
        _campaignStore = new CampaignStore();
        _bidStore = new BidStore();

        foreach (var campaign in campaigns)
        {
            _campaignStore.AddCampaign(campaign);
        }
    }

    // Subscribes to an SSP
    public void SubscribeTo(Ssp ssp)
    {
        Console.WriteLine($"[DSP {_name}] Subscribing to SSP");
        ssp.NewBidRequest += OnBidRequestReceived;
        ssp.SubscribeToFeedback(_name, OnFeedbackReceived);
    }

    public void OnBidRequestReceived(object? sender, BidRequest request)
    {
        try
        {
            Console.WriteLine($"[DSP {_name}] Received BidRequest for {request.UserId}");

            var userData = _userStore.GetUserById(request.UserId);
            if (userData == null)
            {
                Console.WriteLine($"[DSP {_name}] No user data found for {request.UserId}, skipping bid");
                return;
            }

            var (campaignId, bidAmount) = EvaluateBestBid(request);

            if (campaignId == Guid.Empty)
            {
                Console.WriteLine($"[DSP {_name}] No matching campaign found for user {request.UserId}");
                return;
            }

            var bidRecord = new BidRecord
            {
                BidId = request.BidId,
                CampaignId = campaignId,
                BidAmount = bidAmount
            };
            _bidStore.AddBid(bidRecord);

            Console.WriteLine($"[DSP {_name}] Bidding {bidAmount} for campaign {campaignId}");

            // Spend budget when bidding, will be refunded if bid is not won
            var chosenCampaign = _campaignStore.GetCampaignById(campaignId);
            if (chosenCampaign != null)
            {
                var success = chosenCampaign.SpendBudget(bidAmount);
                if (!success)
                {
                    Console.WriteLine($"[DSP {_name}] Failed to bid due to insufficient budget for campaign {campaignId}");
                    return;
                }
            }

            var decision = new BidDecision(request.BidId, _name, bidAmount);
            if (sender is Ssp ssp)
            {
                ssp.ReceiveBidDecision(decision);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DSP {_name}] Error processing bid request {request.BidId}: {ex}");
        }
    }

    public void OnFeedbackReceived(BidFeedback feedback)
    {
        try
        {
            var result = feedback.Win ? "WON" : "LOST";
            _bidStore.AddFeedbackResult(feedback);
            Console.WriteLine($"[DSP {_name}] Feedback: Bid {feedback.BidId} - {result}");

            if (feedback.Win) return;

            // Only refund if bid was lost
            var bid = _bidStore.GetBidById(feedback.BidId);
            Debug.Assert(bid != null, nameof(bid) + " != null");

            var campaign = _campaignStore.GetCampaignById(bid.CampaignId);
            if (campaign == null) return;
            campaign.RefundBudget(bid.BidAmount);
            // Console.WriteLine($"[DSP {_name}] Refunded {bid.BidAmount} to campaign {bid.CampaignId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DSP {_name}] Error processing feedback for bid {feedback.BidId}: {ex}");
        }
    }

    private (Guid CampaignId, decimal BidAmount) EvaluateBestBid(BidRequest bidRequest)
    {
        var userData = _userStore.GetUserById(bidRequest.UserId);
        var bestBid = decimal.MinValue;
        var bestCampaignId = Guid.Empty;

        var allCampaigns = _campaignStore.GetAll();

        foreach (var campaign in allCampaigns)
        {
            var campaignBidAmount = decimal.MinValue;

            foreach (var bidLine in campaign.BidLines)
            {
                if (DoesUserMatchBidLine(userData, bidLine))
                {
                    campaignBidAmount = CalculateBidAmount(campaign, bidLine);
                    break;
                }
            }

            if (campaignBidAmount == decimal.MinValue || campaignBidAmount > campaign.RemainingBudget)
                continue;

            if (campaignBidAmount <= bestBid) continue;
            bestBid = campaignBidAmount;
            bestCampaignId = campaign.CampaignId;
        }

        return (bestCampaignId, bestBid);
    }

    private static bool DoesUserMatchBidLine(UserData? user, BidLine bidLine)
    {
        if (user == null) return false;
        var userFlags = TargetingExtensions.ToFlags(user.TargetingData);
        var bidFlags = TargetingExtensions.ToFlags(bidLine.TargetingData);

        return (userFlags & bidFlags) == bidFlags;
    }

    private static decimal CalculateBidAmount(CampaignDetail campaign, BidLine bidLine)
    {
        return campaign.BaseBid * bidLine.BidFactor;
    }
}
