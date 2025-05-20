using DSP.Api.Models;
using DSP.Api.Stores;
using Shared.Models;

namespace DSP.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<IUserStore, UserStore>();
        builder.Services.AddSingleton<ICampaignStore, CampaignStore>();
        builder.Services.AddSingleton<IBidStore, BidStore>();

        // swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapGet("/", () => Results.Redirect("/swagger"));

        app.MapPost("/users", (UserData user, IUserStore store) =>
        {
            store.AddUser(user);
            return Results.Ok(user);
        });

        app.MapGet("/users/{id}", (string id, IUserStore store) =>
        {
            var user = store.GetUserById(id);
            return user is not null ? Results.Ok(user) : Results.NotFound();
        });

        app.MapPost("/campaigns", (CampaignDetail campaign, ICampaignStore store) =>
        {
            store.AddCampaign(campaign);
            return Results.Ok(campaign);
        });

        app.MapGet("/campaigns/{id:guid}", (Guid id, ICampaignStore store) =>
        {
            var campaign = store.GetCampaignById(id);
            return campaign is not null ? Results.Ok(campaign) : Results.NotFound();
        });

        app.MapPatch("/campaigns/{id:guid}/budget", (Guid id, Decimal newBudget, ICampaignStore store) =>
        {
            var campaign = store.GetCampaignById(id);
            if (campaign == null) return Results.NotFound();

            campaign.UpdateBudget(newBudget);
            return Results.NoContent();
        });

        app.MapPost("/bid", (BidRequest request, IUserStore userStore, ICampaignStore campaignStore, IBidStore bidStore) =>
        {
            var user = userStore.GetUserById(request.UserId);
            if (user == null) return Results.BadRequest("Unknown user");

            var bestBid = decimal.MinValue;
            Guid bestCampaignId = Guid.Empty;

            foreach (var campaign in campaignStore.GetAll())
            {
                foreach (var bidLine in campaign.BidLines)
                {
                    if ((TargetingExtensions.ToFlags(user.TargetingData) & TargetingExtensions.ToFlags(bidLine.TargetingData)) == TargetingExtensions.ToFlags(bidLine.TargetingData))
                    {
                        var bidAmount = campaign.BaseBid * bidLine.BidFactor;
                        if (bidAmount <= campaign.RemainingBudget && bidAmount > bestBid)
                        {
                            bestBid = bidAmount;
                            bestCampaignId = campaign.CampaignId;
                        }
                        break;
                    }
                }
            }

            if (bestCampaignId == Guid.Empty) return Results.NoContent();

            var bid = new BidRecord
            {
                BidId = request.BidId,
                CampaignId = bestCampaignId,
                BidAmount = bestBid
            };
            bidStore.AddBid(bid);

            // Spend budget when bidding, will be refunded if bid is not won
            var chosenCampaign = campaignStore.GetCampaignById(bestCampaignId);
            if (chosenCampaign != null)
            {
                var success = chosenCampaign.SpendBudget(bestBid);
                if (!success)
                {
                    return Results.BadRequest("Insufficient budget");
                }
            };

            return Results.Ok(new BidDecision(request.BidId, "DSP", bestBid));
        });

        app.MapPost("/feedback", (BidFeedback feedback, ICampaignStore campaignStore, IBidStore bidStore) =>
        {
            bidStore.AddFeedbackResult(feedback);

            if (!feedback.Win)
            {
                var bid = bidStore.GetBidById(feedback.BidId);
                var campaign = campaignStore.GetCampaignById(bid.CampaignId);
                campaign?.RefundBudget(bid.BidAmount);
            }

            return Results.NoContent();
        });

        app.Run();
    }
}
