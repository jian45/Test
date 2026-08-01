using System.Collections.Generic;
using CatCafe.MiniGame.Contracts.Commercialization;

namespace CatCafe.MiniGame.Commercialization.Ads
{
    public sealed class RewardGrantCoordinator
    {
        private readonly HashSet<string> _settledRewardRequestIds = new HashSet<string>();
        private readonly object _lock = new object();

        public RewardedAdResult SettleOnce(RewardedAdRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RewardRequestId))
            {
                return RewardedAdResult.FromRequest(
                    request,
                    RewardedAdResultCode.ConfigMissing,
                    "Reward request id is missing.");
            }

            lock (_lock)
            {
                if (_settledRewardRequestIds.Contains(request.RewardRequestId))
                {
                    return RewardedAdResult.FromRequest(
                        request,
                        RewardedAdResultCode.PolicyBlocked,
                        "Reward request has already been settled.");
                }

                _settledRewardRequestIds.Add(request.RewardRequestId);
            }

            return RewardedAdResult.FromRequest(
                request,
                RewardedAdResultCode.Granted,
                "Mock reward settled once.",
                true);
        }

        public bool HasSettled(string rewardRequestId)
        {
            if (string.IsNullOrWhiteSpace(rewardRequestId))
                return false;

            lock (_lock)
            {
                return _settledRewardRequestIds.Contains(rewardRequestId);
            }
        }
    }
}
