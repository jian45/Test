using System.Threading.Tasks;
using CatCafe.MiniGame.Config;
using CatCafe.MiniGame.Contracts.Telemetry;
using UnityEngine;

namespace CatCafe.MiniGame.Contracts.Resource
{
    public interface IMiniGameResourceService
    {
        bool IsInitialized { get; }
        string LastError { get; }

        Task<bool> InitializeAsync(MiniGameRuntimeConfig config, IMiniGameTelemetry telemetry);
        Task<bool> WarmupFirstPlayResourcesAsync();
        Task<T> LoadAssetAsync<T>(string address) where T : Object;
        void ReleaseAsset(string address);
        void ReleaseAll();
    }
}
