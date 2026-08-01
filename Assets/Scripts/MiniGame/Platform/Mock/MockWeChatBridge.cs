using System.Collections.Generic;
using System.Threading.Tasks;
using CatCafe.MiniGame.Contracts.Platform;
using UnityEngine;

namespace CatCafe.MiniGame.Platform.Mock
{
    public sealed class MockWeChatBridge : IWeChatBridge
    {
        private readonly Dictionary<string, string> _launchOptions = new Dictionary<string, string>
        {
            { "scene", "EditorMock" },
            { "query", string.Empty }
        };

        public bool IsInitialized { get; private set; }
        public string PlatformName { get { return "MockWeChatBridge"; } }

        public Task<bool> InitializeAsync()
        {
            IsInitialized = true;
            Debug.Log("[MiniGame][Bridge] Mock bridge initialized.");
            return Task.FromResult(true);
        }

        public bool CanIUse(string apiName)
        {
            return !string.IsNullOrWhiteSpace(apiName);
        }

        public Task<bool> ReportGameStartAsync()
        {
            Debug.Log("[MiniGame][Bridge] Mock report game start.");
            return Task.FromResult(true);
        }

        public string GetLaunchOption(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;

            return _launchOptions.TryGetValue(key, out string value) ? value : string.Empty;
        }
    }
}
