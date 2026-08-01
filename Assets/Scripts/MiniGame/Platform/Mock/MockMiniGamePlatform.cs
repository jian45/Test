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

namespace CatCafe.MiniGame.Platform.Mock
{
    public sealed class MockMiniGamePlatform : IMiniGamePlatform
    {
        public MockMiniGamePlatform()
        {
            RuntimeConfig = MiniGameConfigDefaults.CreateSafeDefaults();
            Bridge = new MockWeChatBridge();
            Telemetry = new MiniGameTelemetryService();
            Resources = new MockMiniGameResourceService();
            RewardedAds = new MockRewardedAdService();
        }

        public bool IsInitialized { get; private set; }
        public bool IsMock { get { return true; } }
        public string EnvironmentName { get { return RuntimeConfig != null ? RuntimeConfig.EnvironmentName : "EditorMock"; } }
        public MiniGameRuntimeConfig RuntimeConfig { get; private set; }
        public IWeChatBridge Bridge { get; private set; }
        public IRewardedAdService RewardedAds { get; private set; }
        public IMiniGameResourceService Resources { get; private set; }
        public IMiniGameTelemetry Telemetry { get; private set; }

        public async Task<bool> InitializeAsync(MiniGameRuntimeConfig config)
        {
            RuntimeConfig = config ?? MiniGameConfigDefaults.CreateSafeDefaults();
            bool bridgeInitialized = await Bridge.InitializeAsync();
            IsInitialized = bridgeInitialized;

            Debug.Log("[MiniGame][Platform] Mock platform initialized=" + IsInitialized);
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
