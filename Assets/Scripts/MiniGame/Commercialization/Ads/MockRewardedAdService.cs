using System.Collections.Generic;
using System.Threading.Tasks;
using CatCafe.MiniGame.Config;
using CatCafe.MiniGame.Contracts.Commercialization;
using CatCafe.MiniGame.Contracts.Telemetry;
using UnityEngine;

namespace CatCafe.MiniGame.Commercialization.Ads
{
    public sealed class MockRewardedAdService : IRewardedAdService
    {
        private readonly RewardGrantCoordinator _grantCoordinator;
        private readonly AdPolicyGate _policyGate;
        private MiniGameRuntimeConfig _config;
        private IMiniGameTelemetry _telemetry;
        private RewardedAdResultCode _nextMockResult = RewardedAdResultCode.Granted;

        public MockRewardedAdService()
            : this(new RewardGrantCoordinator(), new AdPolicyGate())
        {
        }

        public MockRewardedAdService(RewardGrantCoordinator grantCoordinator, AdPolicyGate policyGate)
        {
            _grantCoordinator = grantCoordinator ?? new RewardGrantCoordinator();
            _policyGate = policyGate ?? new AdPolicyGate();
        }

        public bool IsInitialized { get; private set; }
        public bool IsBusy { get; private set; }

        public Task<bool> InitializeAsync(MiniGameRuntimeConfig config, IMiniGameTelemetry telemetry)
        {
            _config = config ?? MiniGameConfigDefaults.CreateSafeDefaults();
            _telemetry = telemetry;
            _nextMockResult = _config.MockAdNextResult;
            IsInitialized = true;

            Debug.Log("[MiniGame][Ads] Mock rewarded ad service initialized.");
            return Task.FromResult(true);
        }

        public Task<RewardedAdResult> PreloadAsync(string placementId)
        {
            RewardedAdPlacementConfig placement = _config != null ? _config.FindRewardedPlacement(placementId) : null;
            if (placement == null)
            {
                RewardedAdResult missing = RewardedAdResult.Create(
                    RewardedAdResultCode.ConfigMissing,
                    placementId,
                    string.Empty,
                    string.Empty,
                    "Placement config is missing.");
                Track(AdTelemetryEvents.LoadFailed, missing);
                return Task.FromResult(missing);
            }

            Track(AdTelemetryEvents.LoadStart, placementId, placement.RewardId, string.Empty, "mock_preload");

            if (_nextMockResult == RewardedAdResultCode.LoadFailed)
            {
                _nextMockResult = RewardedAdResultCode.Granted;
                RewardedAdResult failed = RewardedAdResult.Create(
                    RewardedAdResultCode.LoadFailed,
                    placement.PlacementId,
                    placement.RewardId,
                    string.Empty,
                    "Mock preload failed.");
                Track(AdTelemetryEvents.LoadFailed, failed);
                return Task.FromResult(failed);
            }

            RewardedAdResult success = RewardedAdResult.Create(
                RewardedAdResultCode.Granted,
                placement.PlacementId,
                placement.RewardId,
                string.Empty,
                "Mock preload completed.");
            Track(AdTelemetryEvents.LoadSuccess, success);
            return Task.FromResult(success);
        }

        public Task<RewardedAdResult> ShowAsync(RewardedAdRequest request)
        {
            if (!IsInitialized)
            {
                RewardedAdResult unsupported = RewardedAdResult.FromRequest(
                    request,
                    RewardedAdResultCode.Unsupported,
                    "Rewarded ad service is not initialized.");
                Track(AdTelemetryEvents.ShowFailed, unsupported);
                return Task.FromResult(unsupported);
            }

            Track(AdTelemetryEvents.Request, request);

            RewardedAdResult blocked = _policyGate.Evaluate(request, _config, IsBusy);
            if (blocked != null)
            {
                Track(
                    blocked.Code == RewardedAdResultCode.Busy ? AdTelemetryEvents.ShowFailed : AdTelemetryEvents.PolicyBlocked,
                    blocked);
                return Task.FromResult(blocked);
            }

            IsBusy = true;
            _policyGate.RecordAccepted(request.PlacementId);
            Track(AdTelemetryEvents.ShowStart, request);

            RewardedAdResult result = ResolveMockResult(request);
            IsBusy = false;

            switch (result.Code)
            {
                case RewardedAdResultCode.Granted:
                    Track(AdTelemetryEvents.CloseCompleted, result);
                    Track(result.RewardSettled ? AdTelemetryEvents.RewardGranted : AdTelemetryEvents.RewardFailed, result);
                    break;
                case RewardedAdResultCode.Skipped:
                    Track(AdTelemetryEvents.CloseSkipped, result);
                    break;
                case RewardedAdResultCode.LoadFailed:
                    Track(AdTelemetryEvents.LoadFailed, result);
                    break;
                case RewardedAdResultCode.ShowFailed:
                case RewardedAdResultCode.Unsupported:
                case RewardedAdResultCode.Busy:
                case RewardedAdResultCode.ConfigMissing:
                case RewardedAdResultCode.PolicyBlocked:
                    Track(AdTelemetryEvents.ShowFailed, result);
                    break;
            }

            return Task.FromResult(result);
        }

        public void SetNextMockResult(RewardedAdResultCode resultCode)
        {
            _nextMockResult = resultCode;
        }

        private RewardedAdResult ResolveMockResult(RewardedAdRequest request)
        {
            RewardedAdResultCode resultCode = _nextMockResult;
            _nextMockResult = RewardedAdResultCode.Granted;

            switch (resultCode)
            {
                case RewardedAdResultCode.Granted:
                    return _grantCoordinator.SettleOnce(request);
                case RewardedAdResultCode.Skipped:
                    return RewardedAdResult.FromRequest(request, RewardedAdResultCode.Skipped, "Mock ad skipped by user.");
                case RewardedAdResultCode.LoadFailed:
                    return RewardedAdResult.FromRequest(request, RewardedAdResultCode.LoadFailed, "Mock ad load failed.");
                case RewardedAdResultCode.ShowFailed:
                    return RewardedAdResult.FromRequest(request, RewardedAdResultCode.ShowFailed, "Mock ad show failed.");
                case RewardedAdResultCode.Unsupported:
                    return RewardedAdResult.FromRequest(request, RewardedAdResultCode.Unsupported, "Mock platform reports rewarded ads unsupported.");
                case RewardedAdResultCode.Busy:
                    return RewardedAdResult.FromRequest(request, RewardedAdResultCode.Busy, "Mock ad service is busy.");
                case RewardedAdResultCode.ConfigMissing:
                    return RewardedAdResult.FromRequest(request, RewardedAdResultCode.ConfigMissing, "Mock ad config missing.");
                case RewardedAdResultCode.PolicyBlocked:
                    return RewardedAdResult.FromRequest(request, RewardedAdResultCode.PolicyBlocked, "Mock policy blocked the ad.");
                default:
                    return RewardedAdResult.FromRequest(request, RewardedAdResultCode.ShowFailed, "Unknown mock ad result.");
            }
        }

        private void Track(string eventName, RewardedAdRequest request)
        {
            if (request == null)
            {
                Track(eventName, string.Empty, string.Empty, string.Empty, "request_missing");
                return;
            }

            Track(eventName, request.PlacementId, request.RewardId, request.RewardRequestId, request.Scene);
        }

        private void Track(string eventName, RewardedAdResult result)
        {
            if (result == null)
            {
                Track(eventName, string.Empty, string.Empty, string.Empty, "result_missing");
                return;
            }

            Track(eventName, result.PlacementId, result.RewardId, result.RewardRequestId, result.Code.ToString());
        }

        private void Track(string eventName, string placementId, string rewardId, string rewardRequestId, string detail)
        {
            if (_telemetry == null)
                return;

            _telemetry.TrackEvent(
                eventName,
                new Dictionary<string, string>
                {
                    { "placementId", placementId ?? string.Empty },
                    { "rewardId", rewardId ?? string.Empty },
                    { "rewardRequestId", rewardRequestId ?? string.Empty },
                    { "detail", detail ?? string.Empty }
                });
        }
    }
}
