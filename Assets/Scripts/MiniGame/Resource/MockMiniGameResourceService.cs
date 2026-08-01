using System.Threading.Tasks;
using CatCafe.MiniGame.Config;
using CatCafe.MiniGame.Contracts.Resource;
using CatCafe.MiniGame.Contracts.Telemetry;
using UnityEngine;

namespace CatCafe.MiniGame.Resource
{
    public sealed class MockMiniGameResourceService : IMiniGameResourceService
    {
        private MiniGameRuntimeConfig _config;
        private IMiniGameTelemetry _telemetry;

        public bool IsInitialized { get; private set; }
        public string LastError { get; private set; }

        public Task<bool> InitializeAsync(MiniGameRuntimeConfig config, IMiniGameTelemetry telemetry)
        {
            _config = config ?? MiniGameConfigDefaults.CreateSafeDefaults();
            _telemetry = telemetry;
            LastError = string.Empty;

            if (_config.SimulateResourceInitializeFailure)
            {
                IsInitialized = false;
                LastError = "Mock resource initialization failed by config.";
                Debug.LogWarning("[MiniGame][Resource] " + LastError);
                _telemetry?.TrackError("MockResourceInitialize", LastError);
                return Task.FromResult(false);
            }

            IsInitialized = true;
            Debug.Log("[MiniGame][Resource] Mock resource service initialized.");
            return Task.FromResult(true);
        }

        public Task<bool> WarmupFirstPlayResourcesAsync()
        {
            if (!IsInitialized)
            {
                LastError = "Mock resource service is not initialized.";
                Debug.LogWarning("[MiniGame][Resource] " + LastError);
                _telemetry?.TrackError("MockResourceWarmup", LastError);
                return Task.FromResult(false);
            }

            if (_config != null && _config.SimulateResourceWarmupFailure)
            {
                LastError = "Mock first play resource warmup failed by config.";
                Debug.LogWarning("[MiniGame][Resource] " + LastError);
                _telemetry?.TrackError("MockResourceWarmup", LastError);
                return Task.FromResult(false);
            }

            LastError = string.Empty;
            Debug.Log("[MiniGame][Resource] Mock first play resources warmed up.");
            return Task.FromResult(true);
        }

        public Task<T> LoadAssetAsync<T>(string address) where T : Object
        {
            LastError = "Mock resource service does not load runtime assets. Address: " + (address ?? string.Empty);
            Debug.Log("[MiniGame][Resource] " + LastError);
            return Task.FromResult<T>(null);
        }

        public void ReleaseAsset(string address)
        {
            Debug.Log("[MiniGame][Resource] Mock resource release asset: " + (address ?? string.Empty));
        }

        public void ReleaseAll()
        {
            Debug.Log("[MiniGame][Resource] Mock resource release all.");
        }
    }
}
