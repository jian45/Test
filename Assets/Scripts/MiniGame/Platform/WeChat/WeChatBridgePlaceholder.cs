using System.Threading.Tasks;
using CatCafe.MiniGame.Contracts.Platform;
using UnityEngine;

namespace CatCafe.MiniGame.Platform.WeChat
{
    public sealed class WeChatBridgePlaceholder : IWeChatBridge
    {
        public bool IsInitialized { get { return false; } }
        public string PlatformName { get { return "WeChatBridgePlaceholder"; } }

        public Task<bool> InitializeAsync()
        {
            Debug.LogWarning("[MiniGame][Bridge] WeChat bridge placeholder is active. Real WXSDK calls are unavailable.");
            return Task.FromResult(false);
        }

        public bool CanIUse(string apiName)
        {
            return false;
        }

        public Task<bool> ReportGameStartAsync()
        {
            Debug.LogWarning("[MiniGame][Bridge] Game start report skipped by placeholder bridge.");
            return Task.FromResult(false);
        }

        public string GetLaunchOption(string key)
        {
            return string.Empty;
        }
    }
}
