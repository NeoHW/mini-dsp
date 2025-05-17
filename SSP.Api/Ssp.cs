using Shared;
using Shared.Models;
using System.Collections.Concurrent;
using System.Timers;
using Timer = System.Timers.Timer;

namespace SSP.Api;

public class Ssp
{
    public const int RequestIntervalInMs = 500;
    public const int ResponseWaitTimeInMs = 200;
    
    
    private readonly Timer _timer;
    private readonly List<UserData> _users;
    public event EventHandler<BidRequest> NewBidRequest;
    private readonly ConcurrentDictionary<string, Action<BidFeedback>> _feedbackSubscribers = new();
    private readonly ConcurrentDictionary<Guid, BidDecision> _bidPool = new(); // Key = RequestId, Value = List of decisions
    private readonly ConcurrentDictionary<Guid, DateTime> _bidDeadlines = new();
    
    public Ssp(IUserStore userStore)
    {
        _users = userStore.GetAllUsersToList();
        _timer = new Timer(RequestIntervalInMs);
        _timer.Elapsed += EmitBidRequest;
        _timer.Start();
    }

    // For DSPs to register callback function to receive their bid feedback
    public void SubscribeToFeedback(string dspName, Action<BidFeedback> handler)
    {
        _feedbackSubscribers[dspName] = handler;
    }

    // For DSPs to send bids back to SSP after receiving bid requests
    public void ReceiveBidDecision(BidDecision decision)
    {
        if (_bidDeadlines.TryGetValue(decision.BidId, out var deadline) && DateTime.UtcNow <= deadline)
        {
            _bidPool.AddOrUpdate(
                decision.BidId,
                decision,
                (_, currentBest) => decision.BidAmount > currentBest.BidAmount ? decision : currentBest);
        }
    }

    private void EmitBidRequest(object? sender, ElapsedEventArgs e)
    {
        var bidId = Guid.NewGuid();
        var user = _users[new Random().Next(_users.Count)];
        var request = new BidRequest(bidId, user.UserId);
        
        _bidDeadlines[bidId] = DateTime.UtcNow.AddMilliseconds(ResponseWaitTimeInMs);

        // Notify all DSPs which subscribed
        NewBidRequest?.Invoke(this, request);

        _ = Task.Run(async () =>
        {
            await Task.Delay(ResponseWaitTimeInMs); // Wait for DSPs to respond 

            if (_bidPool.TryRemove(bidId, out var winner))
            {
                foreach (var (dspName, handler) in _feedbackSubscribers)
                {
                    bool isWin = dspName == winner.DspName;
                    handler(new BidFeedback(bidId, isWin));
                }
                _bidDeadlines.TryRemove(bidId, out _);
            }
        });
    }

    private void NotifyFeedback(string dspName, BidFeedback feedback)
    {
       if (_feedbackSubscribers.TryGetValue(dspName, out var handler))
           handler.Invoke(feedback);
    }
}
