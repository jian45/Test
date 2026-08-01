using System.Threading.Tasks;
using CatCafe.MiniGame.Config;
using CatCafe.MiniGame.Contracts.Commercialization;
using CatCafe.MiniGame.Contracts.Resource;
using CatCafe.MiniGame.Contracts.Telemetry;

namespace CatCafe.MiniGame.Contracts.Platform
{
    public interface IMiniGamePlatform
    {
        bool IsInitialized { get; }
        bool IsMock { get; }
        string EnvironmentName { get; }
        MiniGameRuntimeConfig RuntimeConfig { get; }
        IWeChatBridge Bridge { get; }
        IRewardedAdService RewardedAds { get; }
        IMiniGameResourceService Resources { get; }
        IMiniGameTelemetry Telemetry { get; }

        Task<bool> InitializeAsync(MiniGameRuntimeConfig config);
        string GetLaunchOption(string key);
        bool CanUse(string capabilityName);
    }
}
