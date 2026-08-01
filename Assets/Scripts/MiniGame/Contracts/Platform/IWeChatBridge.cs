using System.Threading.Tasks;

namespace CatCafe.MiniGame.Contracts.Platform
{
    public interface IWeChatBridge
    {
        bool IsInitialized { get; }
        string PlatformName { get; }

        Task<bool> InitializeAsync();
        bool CanIUse(string apiName);
        Task<bool> ReportGameStartAsync();
        string GetLaunchOption(string key);
    }
}
