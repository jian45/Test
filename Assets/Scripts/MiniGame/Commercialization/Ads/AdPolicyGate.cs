using System;
using System.Collections.Generic;
using CatCafe.MiniGame.Config;
using CatCafe.MiniGame.Contracts.Commercialization;

namespace CatCafe.MiniGame.Commercialization.Ads
{
    public sealed class AdPolicyGate
    {
        private readonly Dictionary<string, DateTimeOffset> _lastAcceptedAt = new Dictionary<string, DateTimeOffset>();
        private readonly Dictionary<string, int> _sessionCounts = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _dailyCounts = new Dictionary<string, int>();

        public RewardedAdResult Evaluate(
            RewardedAdRequest request,
            MiniGameRuntimeConfig config,
            bool isBusy)
        {
            if (isBusy)
            {
                return RewardedAdResult.FromRequest(request, RewardedAdResultCode.Busy, "A rewarded ad is already showing.");
            }

            if (request == null ||
                string.IsNullOrWhiteSpace(request.PlacementId) ||
                string.IsNullOrWhiteSpace(request.RewardId) ||
                string.IsNullOrWhiteSpace(request.RewardRequestId))
            {
                return RewardedAdResult.FromRequest(request, RewardedAdResultCode.ConfigMissing, "Rewarded ad request is incomplete.");
            }

            if (config == null)
            {
                return RewardedAdResult.FromRequest(request, RewardedAdResultCode.ConfigMissing, "Runtime config is missing.");
            }

            RewardedAdPlacementConfig placement = config.FindRewardedPlacement(request.PlacementId);
            if (placement == null)
            {
                return RewardedAdResult.FromRequest(request, RewardedAdResultCode.ConfigMissing, "Placement config is missing.");
            }

            if (!placement.Enabled)
            {
                return RewardedAdResult.FromRequest(request, RewardedAdResultCode.PolicyBlocked, "Placement is disabled.");
            }

            if (IsCoolingDown(placement, out int remainingSeconds))
            {
                return RewardedAdResult.FromRequest(
                    request,
                    RewardedAdResultCode.PolicyBlocked,
                    "Placement is cooling down. Remaining seconds: " + remainingSeconds);
            }

            int sessionCount = GetCount(_sessionCounts, placement.PlacementId);
            if (placement.SessionCap > 0 && sessionCount >= placement.SessionCap)
            {
                return RewardedAdResult.FromRequest(request, RewardedAdResultCode.PolicyBlocked, "Session cap reached.");
            }

            int dailyCount = GetCount(_dailyCounts, placement.PlacementId);
            if (placement.DailyCap > 0 && dailyCount >= placement.DailyCap)
            {
                return RewardedAdResult.FromRequest(request, RewardedAdResultCode.PolicyBlocked, "Daily cap reached.");
            }

            return null;
        }

        public void RecordAccepted(string placementId)
        {
            if (string.IsNullOrWhiteSpace(placementId))
                return;

            _lastAcceptedAt[placementId] = DateTimeOffset.UtcNow;
            _sessionCounts[placementId] = GetCount(_sessionCounts, placementId) + 1;
            _dailyCounts[placementId] = GetCount(_dailyCounts, placementId) + 1;
        }

        private bool IsCoolingDown(RewardedAdPlacementConfig placement, out int remainingSeconds)
        {
            remainingSeconds = 0;

            if (placement == null || placement.CooldownSeconds <= 0)
                return false;

            if (!_lastAcceptedAt.TryGetValue(placement.PlacementId, out DateTimeOffset lastAccepted))
                return false;

            double elapsed = (DateTimeOffset.UtcNow - lastAccepted).TotalSeconds;
            if (elapsed >= placement.CooldownSeconds)
                return false;

            remainingSeconds = Math.Max(1, placement.CooldownSeconds - (int)elapsed);
            return true;
        }

        private static int GetCount(Dictionary<string, int> counts, string placementId)
        {
            if (counts == null || string.IsNullOrWhiteSpace(placementId))
                return 0;

            return counts.TryGetValue(placementId, out int count) ? count : 0;
        }
    }
}
