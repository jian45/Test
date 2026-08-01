using System;

namespace CatCafe.MiniGame.Commercialization.Ads
{
    public static class RewardRequestIdGenerator
    {
        public static string Create(string placementId)
        {
            string safePlacement = string.IsNullOrWhiteSpace(placementId) ? "UnknownPlacement" : placementId.Trim();
            return safePlacement + "-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "-" + Guid.NewGuid().ToString("N");
        }
    }
}
