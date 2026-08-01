using System;

namespace CatCafe.MiniGame.Contracts.Commercialization
{
    [Serializable]
    public sealed class RewardedAdRequest
    {
        public string PlacementId;
        public string RewardId;
        public string RewardRequestId;
        public string Scene;
        public long CreatedUnixTime;

        public static RewardedAdRequest Create(
            string placementId,
            string rewardId,
            string rewardRequestId,
            string scene)
        {
            return new RewardedAdRequest
            {
                PlacementId = placementId ?? string.Empty,
                RewardId = rewardId ?? string.Empty,
                RewardRequestId = rewardRequestId ?? string.Empty,
                Scene = scene ?? string.Empty,
                CreatedUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }
    }
}
