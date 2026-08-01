using System;
using System.Collections.Generic;
using CatCafe.MiniGame.Contracts.Commercialization;

namespace CatCafe.MiniGame.Config
{
    [Serializable]
    public sealed class MiniGameRuntimeConfig
    {
        public const string PlaceholderAppId = "PendingOfficialAppId";

        public string EnvironmentName = "EditorMock";
        public bool UseMockPlatform = true;
        public bool AllowRealPlatformCalls = false;
        public string AppId = PlaceholderAppId;
        public string NetworkBaseUrl = string.Empty;
        public bool UseAddressablesResourceService = true;
        public bool UseWeChatRewardedAdService = true;
        public bool PreferWeChatPlatformOnWebGL = true;
        public bool FallbackToMockWhenRealPlatformFails = true;
        public bool SimulateResourceInitializeFailure = false;
        public bool SimulateResourceWarmupFailure = false;
        public bool SimulateTelemetryFailure = false;
        public RewardedAdResultCode MockAdNextResult = RewardedAdResultCode.Granted;
        public List<string> FirstPlayWarmupLabels = new List<string>();
        public List<RewardedAdPlacementConfig> RewardedAdPlacements = new List<RewardedAdPlacementConfig>();

        public RewardedAdPlacementConfig FindRewardedPlacement(string placementId)
        {
            if (string.IsNullOrWhiteSpace(placementId))
                return null;

            for (int i = 0; i < RewardedAdPlacements.Count; i++)
            {
                RewardedAdPlacementConfig placement = RewardedAdPlacements[i];
                if (placement == null)
                    continue;

                if (string.Equals(placement.PlacementId, placementId, StringComparison.OrdinalIgnoreCase))
                    return placement;
            }

            return null;
        }

        public ConfigValidationResult ValidateForPhaseB()
        {
            ConfigValidationResult result = new ConfigValidationResult();

            if (!UseMockPlatform && !AllowRealPlatformCalls)
            {
                result.Add(
                    ConfigValidationSeverity.Warning,
                    "REAL_PLATFORM_DISABLED",
                    "Real platform calls are disabled in phase B; mock platform should remain enabled.");
            }

            if (string.IsNullOrWhiteSpace(AppId) || string.Equals(AppId, PlaceholderAppId, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(
                    ConfigValidationSeverity.Blocking,
                    "APP_ID_PLACEHOLDER",
                    "AppID is empty or placeholder. This is allowed for phase B development but blocks publishing.");
            }

            if (RewardedAdPlacements == null || RewardedAdPlacements.Count == 0)
            {
                result.Add(
                    ConfigValidationSeverity.Warning,
                    "AD_PLACEMENT_EMPTY",
                    "No rewarded ad placements are configured. Mock flow can run, but ad scenarios cannot be validated.");
                return result;
            }

            for (int i = 0; i < RewardedAdPlacements.Count; i++)
            {
                RewardedAdPlacementConfig placement = RewardedAdPlacements[i];
                if (placement == null || string.IsNullOrWhiteSpace(placement.PlacementId))
                {
                    result.Add(ConfigValidationSeverity.Warning, "AD_PLACEMENT_INVALID", "A rewarded ad placement is empty.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(placement.AdUnitId))
                {
                    result.Add(
                        ConfigValidationSeverity.Blocking,
                        "AD_UNIT_ID_EMPTY",
                        "adUnitId is empty for placement " + placement.PlacementId + ". Mock can run, but real ads and publishing are blocked.");
                }
            }

            return result;
        }

        public ConfigValidationResult ValidateForPhaseC()
        {
            ConfigValidationResult result = new ConfigValidationResult();

            if (UseMockPlatform)
            {
                result.Add(
                    ConfigValidationSeverity.Info,
                    "MOCK_PLATFORM_ENABLED",
                    "Mock platform is enabled. This is the default Editor path and remains valid for phase C development.");
            }

            if (!UseMockPlatform && !AllowRealPlatformCalls)
            {
                result.Add(
                    ConfigValidationSeverity.Warning,
                    "REAL_PLATFORM_DISABLED",
                    "Mock is disabled but real platform calls are also disabled. Startup will safely fall back or fail with a clear platform error.");
            }

            if (!UseMockPlatform && AllowRealPlatformCalls && !FallbackToMockWhenRealPlatformFails)
            {
                result.Add(
                    ConfigValidationSeverity.Info,
                    "REAL_PLATFORM_STRICT_MODE",
                    "Real platform strict mode is enabled. If WeChat SDK init fails, startup will enter an observable error state instead of silently entering Ready.");
            }

            if (string.IsNullOrWhiteSpace(AppId) || string.Equals(AppId, PlaceholderAppId, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(
                    ConfigValidationSeverity.Warning,
                    "APP_ID_PLACEHOLDER",
                    "AppID is empty or placeholder. Editor Mock can run, but WebGL, developer tool import, experience build and publishing are blocked.");
            }

            if (RewardedAdPlacements == null || RewardedAdPlacements.Count == 0)
            {
                result.Add(
                    ConfigValidationSeverity.Warning,
                    "AD_PLACEMENT_EMPTY",
                    "No rewarded ad placements are configured. Mock flow can run, but real ad scenarios cannot be validated.");
                return result;
            }

            for (int i = 0; i < RewardedAdPlacements.Count; i++)
            {
                RewardedAdPlacementConfig placement = RewardedAdPlacements[i];
                if (placement == null || string.IsNullOrWhiteSpace(placement.PlacementId))
                {
                    result.Add(ConfigValidationSeverity.Warning, "AD_PLACEMENT_INVALID", "A rewarded ad placement is empty.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(placement.AdUnitId))
                {
                    result.Add(
                        ConfigValidationSeverity.Warning,
                        "AD_UNIT_ID_EMPTY",
                        "adUnitId is empty for placement " + placement.PlacementId + ". Editor Mock can run, but real ads and publishing are blocked.");
                }
            }

            return result;
        }
    }
}
