using System.Collections.Generic;
using System.Threading.Tasks;
using CatCafe.MiniGame.Commercialization.Ads;
using CatCafe.MiniGame.Config;
using CatCafe.MiniGame.Contracts.Commercialization;
using UnityEngine;

namespace CatCafe.MiniGame.App.Bootstrap
{
    [DefaultExecutionOrder(-9000)]
    public sealed class MiniGameBootstrap : MonoBehaviour
    {
        [SerializeField] private bool autoStartOnStart = true;
        [SerializeField] private bool runOnlyOnce = true;
        [SerializeField] private bool useMockPlatform = true;
        [SerializeField] private bool allowRealPlatformCalls = false;
        [SerializeField] private bool preferWeChatPlatformOnWebGL = true;
        [SerializeField] private bool fallbackToMockWhenRealPlatformFails = true;
        [SerializeField] private bool useAddressablesResourceService = true;
        [SerializeField] private bool useWeChatRewardedAdService = true;
        [SerializeField] private string appId = MiniGameRuntimeConfig.PlaceholderAppId;
        [SerializeField] private List<string> firstPlayWarmupLabels = new List<string>();
        [SerializeField] private RewardedAdResultCode nextMockAdResult = RewardedAdResultCode.Granted;

        private bool _hasStarted;
        private StartupFlow _startupFlow;
        private MiniGameRuntimeConfig _runtimeConfig;

        public StartupFlow StartupFlow
        {
            get { return _startupFlow; }
        }

        private async void Start()
        {
            if (autoStartOnStart)
                await StartFlowAsync();
        }

        public async Task<IReadOnlyList<StartupStepResult>> StartFlowAsync()
        {
            if (runOnlyOnce && _hasStarted && _startupFlow != null)
                return _startupFlow.Results;

            _hasStarted = true;
            _runtimeConfig = CreateRuntimeConfig();

            _startupFlow = new StartupFlow(_runtimeConfig);
            IReadOnlyList<StartupStepResult> results = await _startupFlow.ExecuteAsync();

            Debug.Log("[MiniGame][Bootstrap] Startup flow completed. Step count: " + results.Count);
            return results;
        }

        public async Task<RewardedAdResult> ShowMockRewardedAdAsync(string placementId, string scene)
        {
            if (_startupFlow == null || _startupFlow.Platform == null)
                await StartFlowAsync();

            RewardedAdPlacementConfig placement = _runtimeConfig.FindRewardedPlacement(placementId);
            string rewardId = placement != null ? placement.RewardId : string.Empty;

            RewardedAdRequest request = RewardedAdRequest.Create(
                placementId,
                rewardId,
                RewardRequestIdGenerator.Create(placementId),
                scene);

            return await _startupFlow.Platform.RewardedAds.ShowAsync(request);
        }

        public void SetNextMockAdResult(RewardedAdResultCode resultCode)
        {
            nextMockAdResult = resultCode;

            if (_startupFlow != null && _startupFlow.Platform != null && _startupFlow.Platform.RewardedAds != null)
                _startupFlow.Platform.RewardedAds.SetNextMockResult(resultCode);
        }

        private MiniGameRuntimeConfig CreateRuntimeConfig()
        {
            MiniGameRuntimeConfig config = MiniGameConfigDefaults.CreateSafeDefaults();
            config.UseMockPlatform = useMockPlatform;
            config.AllowRealPlatformCalls = allowRealPlatformCalls;
            config.PreferWeChatPlatformOnWebGL = preferWeChatPlatformOnWebGL;
            config.FallbackToMockWhenRealPlatformFails = fallbackToMockWhenRealPlatformFails;
            config.UseAddressablesResourceService = useAddressablesResourceService;
            config.UseWeChatRewardedAdService = useWeChatRewardedAdService;
            config.AppId = string.IsNullOrWhiteSpace(appId) ? MiniGameRuntimeConfig.PlaceholderAppId : appId;
            config.FirstPlayWarmupLabels = firstPlayWarmupLabels != null
                ? new List<string>(firstPlayWarmupLabels)
                : new List<string>();
            config.MockAdNextResult = nextMockAdResult;
            config.EnvironmentName = useMockPlatform ? "EditorMock" : "WeChatWebGL";
            return config;
        }
    }
}
