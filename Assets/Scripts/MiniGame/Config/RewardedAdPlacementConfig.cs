using System;

namespace CatCafe.MiniGame.Config
{
    [Serializable]
    public sealed class RewardedAdPlacementConfig
    {
        public string PlacementId;
        public string AdUnitId;
        public bool Enabled = true;
        public bool PreloadOnBoot = true;
        public int CooldownSeconds = 30;
        public int SessionCap = 3;
        public int DailyCap = 20;
        public string RewardId;
        public string Fallback;
        public string TelemetryTag;

        public bool HasAdUnitId
        {
            get { return !string.IsNullOrWhiteSpace(AdUnitId); }
        }
    }
}
