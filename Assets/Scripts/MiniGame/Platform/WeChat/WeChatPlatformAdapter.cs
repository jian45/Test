using System.Threading.Tasks;
using CatCafe.MiniGame.Commercialization.Ads;
using CatCafe.MiniGame.Config;
using CatCafe.MiniGame.Contracts.Commercialization;
using CatCafe.MiniGame.Contracts.Platform;
using CatCafe.MiniGame.Contracts.Resource;
using CatCafe.MiniGame.Contracts.Telemetry;
using CatCafe.MiniGame.Resource;
using CatCafe.MiniGame.Telemetry;
using UnityEngine;

namespace CatCafe.MiniGame.Platform.WeChat
{
    public sealed class WeChatPlatformAdapter : IMiniGamePlatform
    {
        private readonly WeChatBridge _bridge;

        public WeChatPlatformAdapter()
        {
            RuntimeConfig = MiniGameConfigDefaults.CreateSafeDefaults();
            _bridge = new WeChatBridge();
            Bridge = _bridge;
            Telemetry = new MiniGameTelemetryService();
            Resources = new AddressablesMiniGameResourceService();
            RewardedAds = new WeChatRewardedAdService(_bridge);
        }

        public bool IsInitialized { get; private set; }
        public bool IsMock { get { return false; } }
        public string EnvironmentName { get { return RuntimeConfig != null ? RuntimeConfig.EnvironmentName : "WeChat"; } }
        public MiniGameRuntimeConfig RuntimeConfig { get; private set; }
        public IWeChatBridge Bridge { get; private set; }
        public IRewardedAdService RewardedAds { get; private set; }
        public IMiniGameResourceService Resources { get; private set; }
        public IMiniGameTelemetry Telemetry { get; private set; }

        public async Task<bool> InitializeAsync(MiniGameRuntimeConfig config)
        {
            RuntimeConfig = config ?? MiniGameConfigDefaults.CreateSafeDefaults();
            Resources = RuntimeConfig.UseAddressablesResourceService
                ? (IMiniGameResourceService)new AddressablesMiniGameResourceService()
                : new MockMiniGameResourceService();
            RewardedAds = RuntimeConfig.UseWeChatRewardedAdService
                ? (IRewardedAdService)new WeChatRewardedAdService(_bridge)
                : new MockRewardedAdService();

            bool initialized = await Bridge.InitializeAsync();
            IsInitialized = initialized;

            if (!initialized)
                Debug.LogWarning("[MiniGame][Platform] WeChat platform initialization failed. Mock remains available for Editor and forced-mock paths.");
            else
                Debug.Log("[MiniGame][Platform] WeChat platform initialized.");

            return IsInitialized;
        }

        public string GetLaunchOption(string key)
        {
            return Bridge != null ? Bridge.GetLaunchOption(key) : string.Empty;
        }

        public bool CanUse(string capabilityName)
        {
            return Bridge != null && Bridge.CanIUse(capabilityName);
        }
    }
}
