using System.Collections.Generic;

namespace CatCafe.MiniGame.Config
{
    public static class MiniGameConfigDefaults
    {
        public static MiniGameRuntimeConfig CreateSafeDefaults()
        {
            return new MiniGameRuntimeConfig
            {
                EnvironmentName = "EditorMock",
                UseMockPlatform = true,
                AllowRealPlatformCalls = false,
                UseAddressablesResourceService = true,
                UseWeChatRewardedAdService = true,
                PreferWeChatPlatformOnWebGL = true,
                FallbackToMockWhenRealPlatformFails = true,
                AppId = MiniGameRuntimeConfig.PlaceholderAppId,
                NetworkBaseUrl = string.Empty,
                FirstPlayWarmupLabels = new List<string>(),
                RewardedAdPlacements = new List<RewardedAdPlacementConfig>
                {
                    new RewardedAdPlacementConfig
                    {
                        PlacementId = "Revive",
                        AdUnitId = string.Empty,
                        Enabled = true,
                        PreloadOnBoot = true,
                        CooldownSeconds = 30,
                        SessionCap = 2,
                        DailyCap = 10,
                        RewardId = "ReviveOnce",
                        Fallback = "ContinueWithoutRevive",
                        TelemetryTag = "ad_rewarded_revive"
                    },
                    new RewardedAdPlacementConfig
                    {
                        PlacementId = "DoubleReward",
                        AdUnitId = string.Empty,
                        Enabled = true,
                        PreloadOnBoot = true,
                        CooldownSeconds = 20,
                        SessionCap = 3,
                        DailyCap = 20,
                        RewardId = "DoubleSettlementReward",
                        Fallback = "ClaimNormalReward",
                        TelemetryTag = "ad_rewarded_double_reward"
                    },
                    new RewardedAdPlacementConfig
                    {
                        PlacementId = "ClaimIdleReward",
                        AdUnitId = string.Empty,
                        Enabled = true,
                        PreloadOnBoot = false,
                        CooldownSeconds = 60,
                        SessionCap = 2,
                        DailyCap = 10,
                        RewardId = "IdleRewardBoost",
                        Fallback = "ClaimNormalIdleReward",
                        TelemetryTag = "ad_rewarded_idle_reward"
                    }
                }
            };
        }
    }
}
