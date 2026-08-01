using System.Collections.Generic;
using System.Threading.Tasks;
using CatCafe.MiniGame.Commercialization.Ads;
using CatCafe.MiniGame.Config;
using CatCafe.MiniGame.Contracts.Commercialization;
using CatCafe.MiniGame.Contracts.Telemetry;
using UnityEngine;
using WeChatWASM;

namespace CatCafe.MiniGame.Platform.WeChat
{
    public sealed class WeChatRewardedAdService : IRewardedAdService
    {
        private const int AdCallbackTimeoutMilliseconds = 12000;

        private readonly RewardGrantCoordinator _grantCoordinator;
        private readonly AdPolicyGate _policyGate;
        private readonly Dictionary<string, WXRewardedVideoAd> _adsByPlacement = new Dictionary<string, WXRewardedVideoAd>();
        private readonly WeChatBridge _bridge;
        private MiniGameRuntimeConfig _config;
        private IMiniGameTelemetry _telemetry;

        public WeChatRewardedAdService()
            : this(null, new RewardGrantCoordinator(), new AdPolicyGate())
        {
        }

        public WeChatRewardedAdService(WeChatBridge bridge)
            : this(bridge, new RewardGrantCoordinator(), new AdPolicyGate())
        {
        }

        public WeChatRewardedAdService(RewardGrantCoordinator grantCoordinator, AdPolicyGate policyGate)
            : this(null, grantCoordinator, policyGate)
        {
        }

        public WeChatRewardedAdService(WeChatBridge bridge, RewardGrantCoordinator grantCoordinator, AdPolicyGate policyGate)
        {
            _bridge = bridge;
            _grantCoordinator = grantCoordinator ?? new RewardGrantCoordinator();
            _policyGate = policyGate ?? new AdPolicyGate();
        }

        public bool IsInitialized { get; private set; }
        public bool IsBusy { get; private set; }

        public Task<bool> InitializeAsync(MiniGameRuntimeConfig config, IMiniGameTelemetry telemetry)
        {
            _config = config ?? MiniGameConfigDefaults.CreateSafeDefaults();
            _telemetry = telemetry;
            IsInitialized = true;
            Debug.Log("[MiniGame][Ads] WeChat rewarded ad service initialized. Real WX ad calls are enabled only after WX.InitSDK and valid adUnitId config.");
            return Task.FromResult(true);
        }

        public async Task<RewardedAdResult> PreloadAsync(string placementId)
        {
            RewardedAdPlacementConfig placement = _config != null ? _config.FindRewardedPlacement(placementId) : null;
            RewardedAdResult configIssue = ValidatePlacement(placementId, placement, null);
            if (configIssue != null)
                return configIssue;

            RewardedAdResult bridgeIssue = ValidateBridgeReady(
                placement.PlacementId,
                placement.RewardId,
                null);
            if (bridgeIssue != null)
            {
                Track(AdTelemetryEvents.LoadFailed, bridgeIssue);
                return bridgeIssue;
            }

            if (!_bridge.CanUseRewardedVideo())
            {
                RewardedAdResult unsupported = RewardedAdResult.Create(
                    RewardedAdResultCode.Unsupported,
                    placement.PlacementId,
                    placement.RewardId,
                    string.Empty,
                    "WX rewarded video capability is unavailable.");
                Track(AdTelemetryEvents.LoadFailed, unsupported);
                return unsupported;
            }

            WXRewardedVideoAd ad = GetOrCreateAd(placement);
            if (ad == null)
            {
                RewardedAdResult failed = RewardedAdResult.Create(
                    RewardedAdResultCode.LoadFailed,
                    placement.PlacementId,
                    placement.RewardId,
                    string.Empty,
                    "WX.CreateRewardedVideoAd returned null.");
                Track(AdTelemetryEvents.LoadFailed, failed);
                return failed;
            }

            Track(AdTelemetryEvents.LoadStart, RewardedAdResult.Create(
                RewardedAdResultCode.LoadFailed,
                placement.PlacementId,
                placement.RewardId,
                string.Empty,
                "Preload start."));

            AdLoadOutcome loadOutcome = await LoadAdAsync(ad, placement, null);
            if (!loadOutcome.Succeeded)
            {
                Track(AdTelemetryEvents.LoadFailed, loadOutcome.FailureResult);
                return loadOutcome.FailureResult;
            }

            RewardedAdResult loaded = RewardedAdResult.Create(
                RewardedAdResultCode.Loaded,
                placement.PlacementId,
                placement.RewardId,
                string.Empty,
                loadOutcome.Message);
            Track(AdTelemetryEvents.LoadSuccess, loaded);
            return loaded;
        }

        public async Task<RewardedAdResult> ShowAsync(RewardedAdRequest request)
        {
            if (!IsInitialized)
            {
                RewardedAdResult _unsupported = RewardedAdResult.FromRequest(
                    request,
                    RewardedAdResultCode.Unsupported,
                    "WeChat rewarded ad service is not initialized.");
                Track(AdTelemetryEvents.ShowFailed, _unsupported);
                return _unsupported;
            }

            Track(AdTelemetryEvents.Request, request);

            RewardedAdResult blocked = _policyGate.Evaluate(request, _config, IsBusy);
            if (blocked != null)
            {
                Track(
                    blocked.Code == RewardedAdResultCode.Busy ? AdTelemetryEvents.ShowFailed : AdTelemetryEvents.PolicyBlocked,
                    blocked);
                return blocked;
            }

            RewardedAdPlacementConfig placement = _config.FindRewardedPlacement(request.PlacementId);
            RewardedAdResult configIssue = ValidatePlacement(request.PlacementId, placement, request);
            if (configIssue != null)
                return configIssue;

            RewardedAdResult bridgeIssue = ValidateBridgeReady(
                request.PlacementId,
                request.RewardId,
                request);
            if (bridgeIssue != null)
            {
                Track(AdTelemetryEvents.ShowFailed, bridgeIssue);
                return bridgeIssue;
            }

            if (!_bridge.CanUseRewardedVideo())
            {
                RewardedAdResult unsupported = RewardedAdResult.FromRequest(
                    request,
                    RewardedAdResultCode.Unsupported,
                    "WX rewarded video capability is unavailable.");
                Track(AdTelemetryEvents.ShowFailed, unsupported);
                return unsupported;
            }

            WXRewardedVideoAd ad = GetOrCreateAd(placement);
            if (ad == null)
            {
                RewardedAdResult loadFailed = RewardedAdResult.FromRequest(
                    request,
                    RewardedAdResultCode.LoadFailed,
                    "WX.CreateRewardedVideoAd returned null.");
                Track(AdTelemetryEvents.LoadFailed, loadFailed);
                return loadFailed;
            }

            IsBusy = true;
            try
            {
                Track(AdTelemetryEvents.LoadStart, request);
                AdLoadOutcome loadOutcome = await LoadAdAsync(ad, placement, request);
                if (!loadOutcome.Succeeded)
                {
                    Track(AdTelemetryEvents.LoadFailed, loadOutcome.FailureResult);
                    return loadOutcome.FailureResult;
                }

                Track(AdTelemetryEvents.LoadSuccess, RewardedAdResult.FromRequest(
                    request,
                    RewardedAdResultCode.Loaded,
                    loadOutcome.Message));
                Track(AdTelemetryEvents.ShowStart, request);
                return await ShowLoadedAdAsync(ad, request);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void SetNextMockResult(RewardedAdResultCode resultCode)
        {
            Debug.Log("[MiniGame][Ads] SetNextMockResult ignored by WeChatRewardedAdService. Use MockRewardedAdService for deterministic mock outcomes.");
        }

        private WXRewardedVideoAd GetOrCreateAd(RewardedAdPlacementConfig placement)
        {
            if (placement == null)
                return null;

            if (_adsByPlacement.TryGetValue(placement.PlacementId, out WXRewardedVideoAd existing) && existing != null)
                return existing;

            try
            {
                WXRewardedVideoAd created = _bridge.CreateRewardedVideoAd(placement.AdUnitId);
                if (created != null)
                    _adsByPlacement[placement.PlacementId] = created;

                return created;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("[MiniGame][Ads] WX.CreateRewardedVideoAd failed for placement " + placement.PlacementId + ": " + exception.Message);
                return null;
            }
        }

        private async Task<AdLoadOutcome> LoadAdAsync(
            WXRewardedVideoAd ad,
            RewardedAdPlacementConfig placement,
            RewardedAdRequest request)
        {
            TaskCompletionSource<AdLoadOutcome> completion = new TaskCompletionSource<AdLoadOutcome>();

            try
            {
                ad.Load(
                    response =>
                    {
                        completion.TrySetResult(AdLoadOutcome.Success(response != null ? response.errMsg : "Rewarded ad loaded."));
                    },
                    error =>
                    {
                        string message = error != null ? error.errCode + " " + error.errMsg : "Rewarded ad load failed.";
                        RewardedAdResult failure = request != null
                            ? RewardedAdResult.FromRequest(request, RewardedAdResultCode.LoadFailed, message)
                            : RewardedAdResult.Create(RewardedAdResultCode.LoadFailed, placement.PlacementId, placement.RewardId, string.Empty, message);
                        completion.TrySetResult(AdLoadOutcome.Failure(failure));
                    });
            }
            catch (System.Exception exception)
            {
                RewardedAdResult failure = request != null
                    ? RewardedAdResult.FromRequest(request, RewardedAdResultCode.LoadFailed, exception.Message)
                    : RewardedAdResult.Create(RewardedAdResultCode.LoadFailed, placement.PlacementId, placement.RewardId, string.Empty, exception.Message);
                return AdLoadOutcome.Failure(failure);
            }

            Task finished = await Task.WhenAny(completion.Task, Task.Delay(AdCallbackTimeoutMilliseconds));
            if (finished != completion.Task)
            {
                RewardedAdResult failure = request != null
                    ? RewardedAdResult.FromRequest(request, RewardedAdResultCode.LoadFailed, "Rewarded ad load timed out.")
                    : RewardedAdResult.Create(RewardedAdResultCode.LoadFailed, placement.PlacementId, placement.RewardId, string.Empty, "Rewarded ad load timed out.");
                return AdLoadOutcome.Failure(failure);
            }

            return completion.Task.Result;
        }

        private async Task<RewardedAdResult> ShowLoadedAdAsync(WXRewardedVideoAd ad, RewardedAdRequest request)
        {
            TaskCompletionSource<RewardedAdResult> completion = new TaskCompletionSource<RewardedAdResult>();
            System.Action<WXRewardedVideoAdOnCloseResponse> onClose = null;
            System.Action<WXADErrorResponse> onError = null;

            onClose = response =>
            {
                if (response != null && response.isEnded)
                {
                    RewardedAdResult granted = _grantCoordinator.SettleOnce(request);
                    if (granted.Code == RewardedAdResultCode.Granted)
                        granted.Message = "Reward settled once after full WeChat rewarded video view.";

                    completion.TrySetResult(granted);
                    return;
                }

                completion.TrySetResult(RewardedAdResult.FromRequest(
                    request,
                    RewardedAdResultCode.Skipped,
                    "WeChat rewarded video was closed before completion."));
            };

            onError = error =>
            {
                string message = error != null ? error.errCode + " " + error.errMsg : "Rewarded ad show error.";
                completion.TrySetResult(RewardedAdResult.FromRequest(request, RewardedAdResultCode.ShowFailed, message));
            };

            try
            {
                ad.OnClose(onClose);
                ad.OnError(onError);
                ad.Show(
                    response =>
                    {
                        _policyGate.RecordAccepted(request.PlacementId);
                    },
                    response =>
                    {
                        string message = response != null ? response.errCode + " " + response.errMsg : "Rewarded ad show failed.";
                        completion.TrySetResult(RewardedAdResult.FromRequest(request, RewardedAdResultCode.ShowFailed, message));
                    });
            }
            catch (System.Exception exception)
            {
                completion.TrySetResult(RewardedAdResult.FromRequest(request, RewardedAdResultCode.ShowFailed, exception.Message));
            }

            Task finished = await Task.WhenAny(completion.Task, Task.Delay(AdCallbackTimeoutMilliseconds));
            RewardedAdResult result = finished == completion.Task
                ? completion.Task.Result
                : RewardedAdResult.FromRequest(request, RewardedAdResultCode.ShowFailed, "Rewarded ad show timed out.");

            if (onClose != null)
                ad.OffClose(onClose);

            if (onError != null)
                ad.OffError(onError);

            if (result.Code == RewardedAdResultCode.Granted && result.RewardSettled)
            {
                Track(AdTelemetryEvents.CloseCompleted, result);
                Track(AdTelemetryEvents.RewardGranted, result);
            }
            else if (result.Code == RewardedAdResultCode.Skipped)
            {
                Track(AdTelemetryEvents.CloseSkipped, result);
            }
            else
            {
                Track(AdTelemetryEvents.ShowFailed, result);
            }

            return result;
        }

        private RewardedAdResult ValidateBridgeReady(
            string placementId,
            string rewardId,
            RewardedAdRequest request)
        {
            if (_bridge != null && _bridge.IsInitialized)
                return null;

            string message = "WeChat bridge is not initialized. Rewarded video capability checks and WX ad creation are blocked until WX.InitSDK completes.";
            return request != null
                ? RewardedAdResult.FromRequest(request, RewardedAdResultCode.Unsupported, message)
                : RewardedAdResult.Create(RewardedAdResultCode.Unsupported, placementId, rewardId, string.Empty, message);
        }

        private RewardedAdResult ValidatePlacement(
            string placementId,
            RewardedAdPlacementConfig placement,
            RewardedAdRequest request)
        {
            if (placement == null)
            {
                RewardedAdResult missing = request != null
                    ? RewardedAdResult.FromRequest(request, RewardedAdResultCode.ConfigMissing, "Placement config is missing.")
                    : RewardedAdResult.Create(RewardedAdResultCode.ConfigMissing, placementId, string.Empty, string.Empty, "Placement config is missing.");
                Track(AdTelemetryEvents.LoadFailed, missing);
                return missing;
            }

            if (string.IsNullOrWhiteSpace(placement.AdUnitId))
            {
                RewardedAdResult missing = request != null
                    ? RewardedAdResult.FromRequest(request, RewardedAdResultCode.ConfigMissing, "adUnitId is empty for placement " + placement.PlacementId + ".")
                    : RewardedAdResult.Create(RewardedAdResultCode.ConfigMissing, placement.PlacementId, placement.RewardId, string.Empty, "adUnitId is empty.");
                Track(AdTelemetryEvents.LoadFailed, missing);
                return missing;
            }

            return null;
        }

        private void Track(string eventName, RewardedAdResult result)
        {
            if (result == null || _telemetry == null)
                return;

            _telemetry.TrackEvent(
                eventName,
                new Dictionary<string, string>
                {
                    { "placementId", result.PlacementId ?? string.Empty },
                    { "rewardId", result.RewardId ?? string.Empty },
                    { "rewardRequestId", result.RewardRequestId ?? string.Empty },
                    { "detail", result.Code.ToString() }
                });
        }

        private void Track(string eventName, RewardedAdRequest request)
        {
            if (request == null || _telemetry == null)
                return;

            _telemetry.TrackEvent(
                eventName,
                new Dictionary<string, string>
                {
                    { "placementId", request.PlacementId ?? string.Empty },
                    { "rewardId", request.RewardId ?? string.Empty },
                    { "rewardRequestId", request.RewardRequestId ?? string.Empty },
                    { "detail", request.Scene ?? string.Empty }
                });
        }

        private sealed class AdLoadOutcome
        {
            public bool Succeeded;
            public string Message;
            public RewardedAdResult FailureResult;

            public static AdLoadOutcome Success(string message)
            {
                return new AdLoadOutcome
                {
                    Succeeded = true,
                    Message = string.IsNullOrWhiteSpace(message) ? "Rewarded ad loaded." : message
                };
            }

            public static AdLoadOutcome Failure(RewardedAdResult failureResult)
            {
                return new AdLoadOutcome
                {
                    Succeeded = false,
                    Message = failureResult != null ? failureResult.Message : string.Empty,
                    FailureResult = failureResult
                };
            }
        }
    }
}
