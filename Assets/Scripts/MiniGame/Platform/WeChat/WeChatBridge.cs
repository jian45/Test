using System.Threading.Tasks;
using CatCafe.MiniGame.Contracts.Platform;
using UnityEngine;
using WeChatWASM;

namespace CatCafe.MiniGame.Platform.WeChat
{
    public sealed class WeChatBridge : IWeChatBridge
    {
        private const int InitTimeoutMilliseconds = 10000;

        public bool IsInitialized { get; private set; }
        public string PlatformName { get { return "WeChat"; } }
        public string LastError { get; private set; }

        public async Task<bool> InitializeAsync()
        {
            if (IsInitialized)
                return true;

            Debug.Log("[MiniGame][WeChatBridge] WX.InitSDK begin.");

            TaskCompletionSource<int> initCompletion = new TaskCompletionSource<int>();
            try
            {
                WX.InitSDK(code => initCompletion.TrySetResult(code));
            }
            catch (System.Exception exception)
            {
                LastError = "WX.InitSDK threw exception: " + exception.Message;
                Debug.LogWarning("[MiniGame][WeChatBridge] " + LastError);
                return false;
            }

            Task finishedTask = await Task.WhenAny(initCompletion.Task, Task.Delay(InitTimeoutMilliseconds));
            if (finishedTask != initCompletion.Task)
            {
                LastError = "WX.InitSDK timed out after " + InitTimeoutMilliseconds + "ms.";
                Debug.LogWarning("[MiniGame][WeChatBridge] " + LastError);
                return false;
            }

            int initCode = initCompletion.Task.Result;
            IsInitialized = initCode == 0;
            LastError = IsInitialized ? string.Empty : "WX.InitSDK failed with code " + initCode + ".";

            if (IsInitialized)
                Debug.Log("[MiniGame][WeChatBridge] WX.InitSDK completed.");
            else
                Debug.LogWarning("[MiniGame][WeChatBridge] " + LastError);

            return IsInitialized;
        }

        public bool CanIUse(string apiName)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[MiniGame][WeChatBridge] CanIUse skipped before WX.InitSDK completion: " + apiName);
                return false;
            }

            if (string.IsNullOrWhiteSpace(apiName))
                return false;

            try
            {
                return WX.CanIUse(apiName);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("[MiniGame][WeChatBridge] CanIUse failed for " + apiName + ": " + exception.Message);
                return false;
            }
        }

        public Task<bool> ReportGameStartAsync()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[MiniGame][WeChatBridge] Game start report skipped because WeChat bridge is not initialized.");
                return Task.FromResult(false);
            }

            try
            {
                WX.ReportGameStart();
                Debug.Log("[MiniGame][WeChatBridge] WX.ReportGameStart sent.");
                return Task.FromResult(true);
            }
            catch (System.Exception exception)
            {
                LastError = "WX.ReportGameStart failed: " + exception.Message;
                Debug.LogWarning("[MiniGame][WeChatBridge] " + LastError);
                return Task.FromResult(false);
            }
        }

        public string GetLaunchOption(string key)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[MiniGame][WeChatBridge] GetLaunchOption skipped before WX.InitSDK completion: " + key);
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;

            try
            {
                LaunchOptionsGame options = WX.GetLaunchOptionsSync();
                if (options == null)
                    return string.Empty;

                if (string.Equals(key, "scene", System.StringComparison.OrdinalIgnoreCase))
                    return options.scene.ToString(System.Globalization.CultureInfo.InvariantCulture);

                if (string.Equals(key, "shareTicket", System.StringComparison.OrdinalIgnoreCase))
                    return options.shareTicket ?? string.Empty;

                if (string.Equals(key, "hostExtraData", System.StringComparison.OrdinalIgnoreCase))
                    return options.hostExtraData ?? string.Empty;

                const string queryPrefix = "query.";
                if (key.StartsWith(queryPrefix, System.StringComparison.OrdinalIgnoreCase))
                {
                    string queryKey = key.Substring(queryPrefix.Length);
                    return options.query != null && options.query.TryGetValue(queryKey, out string value)
                        ? value ?? string.Empty
                        : string.Empty;
                }

                return options.query != null && options.query.TryGetValue(key, out string directValue)
                    ? directValue ?? string.Empty
                    : string.Empty;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("[MiniGame][WeChatBridge] GetLaunchOption failed for " + key + ": " + exception.Message);
                return string.Empty;
            }
        }

        public bool CanUseRewardedVideo()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[MiniGame][WeChatBridge] Rewarded video capability check skipped before WX.InitSDK completion.");
                return false;
            }

            return CanIUse("CreateRewardedVideoAd") || CanIUse("createRewardedVideoAd");
        }

        public WXRewardedVideoAd CreateRewardedVideoAd(string adUnitId)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[MiniGame][WeChatBridge] CreateRewardedVideoAd skipped before WX.InitSDK completion.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(adUnitId))
                return null;

            try
            {
                return WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam
                {
                    adUnitId = adUnitId,
                    multiton = true
                });
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("[MiniGame][WeChatBridge] WX.CreateRewardedVideoAd failed: " + exception.Message);
                return null;
            }
        }
    }
}
