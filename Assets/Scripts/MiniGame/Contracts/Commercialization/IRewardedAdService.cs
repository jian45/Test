using System.Threading.Tasks;
using CatCafe.MiniGame.Config;
using CatCafe.MiniGame.Contracts.Telemetry;

namespace CatCafe.MiniGame.Contracts.Commercialization
{
    public interface IRewardedAdService
    {
        bool IsInitialized { get; }
        bool IsBusy { get; }

        Task<bool> InitializeAsync(MiniGameRuntimeConfig config, IMiniGameTelemetry telemetry);
        Task<RewardedAdResult> PreloadAsync(string placementId);
        Task<RewardedAdResult> ShowAsync(RewardedAdRequest request);
        void SetNextMockResult(RewardedAdResultCode resultCode);
    }
}
