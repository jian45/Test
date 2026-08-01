using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatCafe.MiniGame.Config;
using CatCafe.MiniGame.Contracts.Platform;
using CatCafe.MiniGame.Platform.Mock;
using CatCafe.MiniGame.Platform.WeChat;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace CatCafe.MiniGame.App.Bootstrap
{
    public sealed class StartupFlow
    {
        private readonly List<StartupStepResult> _results = new List<StartupStepResult>();
        private MiniGameRuntimeConfig _config;
        private IMiniGamePlatform _platform;
        private bool _createdRealPlatform;
        private bool _fellBackToMockAfterRealFailure;
        private bool _hasCriticalFailure;

        public StartupFlow(MiniGameRuntimeConfig config = null)
        {
            _config = config ?? MiniGameConfigDefaults.CreateSafeDefaults();
        }

        public IMiniGamePlatform Platform
        {
            get { return _platform; }
        }

        public IReadOnlyList<StartupStepResult> Results
        {
            get { return _results; }
        }

        public async Task<IReadOnlyList<StartupStepResult>> ExecuteAsync()
        {
            _results.Clear();
            _createdRealPlatform = false;
            _fellBackToMockAfterRealFailure = false;
            _hasCriticalFailure = false;

            await RunStepAsync(StartupStepName.DetectEnvironment, DetectEnvironmentAsync, true);
            await RunStepAsync(StartupStepName.CreatePlatform, CreatePlatformAsync, true);
            await RunStepAsync(StartupStepName.InitializePlatform, InitializePlatformAsync, true);
            await RunStepAsync(StartupStepName.LoadLocalConfig, LoadLocalConfigAsync, true);
            await RunStepAsync(StartupStepName.InitializeTelemetry, InitializeTelemetryAsync, false);
            await RunStepAsync(StartupStepName.InitializeResourceService, InitializeResourceServiceAsync, false);
            await RunStepAsync(StartupStepName.WarmupFirstPlayResources, WarmupFirstPlayResourcesAsync, false);
            await RunStepAsync(StartupStepName.InitializeAds, InitializeAdsAsync, false);
            await RunStepAsync(StartupStepName.ReportGameStart, ReportGameStartAsync, false);
            await RunStepAsync(StartupStepName.EnterReadyState, EnterReadyStateAsync, true);

            return _results;
        }

        private Task<StepOutcome> DetectEnvironmentAsync()
        {
            string detected = Application.isEditor ? "UnityEditor" : Application.platform.ToString();
            Debug.Log("[MiniGame][Startup] DetectEnvironment: " + detected);
            return Task.FromResult(StepOutcome.Success("Detected environment: " + detected));
        }

        private Task<StepOutcome> CreatePlatformAsync()
        {
            if (ShouldUseMockPlatform())
            {
                _platform = new MockMiniGamePlatform();
                _createdRealPlatform = false;
                Debug.Log("[MiniGame][Startup] CreatePlatform: MockMiniGamePlatform");
                return Task.FromResult(StepOutcome.Success("Mock platform created."));
            }

            if (ShouldUseWeChatPlatform())
            {
                _platform = new WeChatPlatformAdapter();
                _createdRealPlatform = true;
                Debug.Log("[MiniGame][Startup] CreatePlatform: WeChatPlatformAdapter");
                return Task.FromResult(StepOutcome.Success("WeChat platform adapter created."));
            }

            _platform = new MockMiniGamePlatform();
            _createdRealPlatform = false;
            Debug.LogWarning("[MiniGame][Startup] CreatePlatform: environment is not eligible for WeChat platform. Falling back to MockMiniGamePlatform.");
            return Task.FromResult(StepOutcome.Success("Mock platform fallback created."));
        }

        private async Task<StepOutcome> InitializePlatformAsync()
        {
            if (_platform == null)
                return StepOutcome.Failure("Platform has not been created.");

            bool initialized = await _platform.InitializeAsync(_config);
            if (initialized)
                return StepOutcome.Success("Platform initialized.");

            if (_createdRealPlatform && _config != null && _config.FallbackToMockWhenRealPlatformFails)
            {
                Debug.LogWarning("[MiniGame][Startup] Real WeChat platform failed to initialize. Falling back to MockMiniGamePlatform because config permits fallback.");
                _platform = new MockMiniGamePlatform();
                _createdRealPlatform = false;

                bool mockInitialized = await _platform.InitializeAsync(_config);
                if (mockInitialized)
                {
                    _fellBackToMockAfterRealFailure = true;
                    return StepOutcome.Success("Real platform failed; Mock fallback initialized and is observable in startup logs.");
                }

                return StepOutcome.Failure("Real platform failed and Mock fallback also failed.");
            }

            return StepOutcome.Failure("Platform initialization failed. Startup will not enter Ready without fallback.");
        }

        private Task<StepOutcome> LoadLocalConfigAsync()
        {
            if (_config == null)
                _config = MiniGameConfigDefaults.CreateSafeDefaults();

            ConfigValidationResult validation = _config.ValidateForPhaseC();
            foreach (ConfigValidationIssue issue in validation.Issues)
            {
                string line = issue.Severity + " " + issue.Code + ": " + issue.Message;
                if (issue.Severity == ConfigValidationSeverity.Blocking)
                    Debug.LogWarning("[MiniGame][Config] " + line);
                else
                    Debug.Log("[MiniGame][Config] " + line);
            }

            return Task.FromResult(StepOutcome.Success("Local config loaded. Issue count: " + validation.Issues.Count));
        }

        private async Task<StepOutcome> InitializeTelemetryAsync()
        {
            if (_platform == null || _platform.Telemetry == null)
                return StepOutcome.Failure("Telemetry service is missing.");

            bool initialized = await _platform.Telemetry.InitializeAsync(_config);
            return initialized
                ? StepOutcome.Success("Telemetry initialized.")
                : StepOutcome.Failure("Telemetry initialization failed, startup continues.");
        }

        private async Task<StepOutcome> InitializeResourceServiceAsync()
        {
            if (_platform == null || _platform.Resources == null)
                return StepOutcome.Failure("Resource service is missing.");

            bool initialized = await _platform.Resources.InitializeAsync(_config, _platform.Telemetry);
            return initialized
                ? StepOutcome.Success("Resource service initialized.")
                : StepOutcome.Failure("Resource service failed, startup continues.");
        }

        private async Task<StepOutcome> WarmupFirstPlayResourcesAsync()
        {
            if (_platform == null || _platform.Resources == null)
                return StepOutcome.Failure("Resource service is missing.");

            bool warmed = await _platform.Resources.WarmupFirstPlayResourcesAsync();
            return warmed
                ? StepOutcome.Success("First play resources warmed up.")
                : StepOutcome.Failure("First play resource warmup failed, startup continues.");
        }

        private async Task<StepOutcome> InitializeAdsAsync()
        {
            if (_platform == null || _platform.RewardedAds == null)
                return StepOutcome.Failure("Rewarded ad service is missing.");

            bool initialized = await _platform.RewardedAds.InitializeAsync(_config, _platform.Telemetry);
            if (!initialized)
                return StepOutcome.Failure("Rewarded ad service failed, startup continues.");

            if (_config.RewardedAdPlacements != null)
            {
                for (int i = 0; i < _config.RewardedAdPlacements.Count; i++)
                {
                    RewardedAdPlacementConfig placement = _config.RewardedAdPlacements[i];
                    if (placement == null || !placement.PreloadOnBoot)
                        continue;

                    await _platform.RewardedAds.PreloadAsync(placement.PlacementId);
                }
            }

            return StepOutcome.Success("Rewarded ad service initialized.");
        }

        private async Task<StepOutcome> ReportGameStartAsync()
        {
            if (_platform == null || _platform.Bridge == null)
                return StepOutcome.Failure("Bridge is missing.");

            bool reported = await _platform.Bridge.ReportGameStartAsync();
            return reported
                ? StepOutcome.Success("Game start reported through platform bridge.")
                : StepOutcome.Failure("Game start report failed, startup continues.");
        }

        private Task<StepOutcome> EnterReadyStateAsync()
        {
            if (_hasCriticalFailure && !_fellBackToMockAfterRealFailure)
            {
                return Task.FromResult(StepOutcome.Failure(
                    "Critical startup failure was recorded. Ready state is blocked until the platform issue is fixed or explicit Mock fallback is enabled."));
            }

            Debug.Log("[MiniGame][Startup] EnterReadyState: MiniGame framework is ready.");
            return Task.FromResult(StepOutcome.Success("MiniGame framework is ready."));
        }

        private bool ShouldUseMockPlatform()
        {
            if (_config == null)
                return true;

            if (Application.isEditor)
                return true;

            if (_config.UseMockPlatform)
                return true;

            return false;
        }

        private bool ShouldUseWeChatPlatform()
        {
            if (_config == null)
                return false;

            if (!_config.AllowRealPlatformCalls)
                return false;

            if (!_config.PreferWeChatPlatformOnWebGL)
                return false;

            return Application.platform == RuntimePlatform.WebGLPlayer;
        }

        private async Task RunStepAsync(StartupStepName stepName, Func<Task<StepOutcome>> action, bool critical)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Debug.Log("[MiniGame][Startup] Begin " + stepName);

            StartupStepResult result;
            try
            {
                StepOutcome outcome = await action();
                stopwatch.Stop();

                result = outcome.Succeeded
                    ? StartupStepResult.Success(stepName, stopwatch.ElapsedMilliseconds, outcome.Message)
                    : StartupStepResult.Failure(stepName, stopwatch.ElapsedMilliseconds, outcome.Message);
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                result = StartupStepResult.Failure(stepName, stopwatch.ElapsedMilliseconds, exception.Message);
            }

            _results.Add(result);
            TrackStartupResult(result);

            string level = result.Succeeded ? "Success" : "Failure";
            Debug.Log("[MiniGame][Startup] " + level + " " + stepName + " " + result.DurationMs + "ms " + result.Message);

            if (!result.Succeeded && critical)
            {
                _hasCriticalFailure = true;
                Debug.LogWarning("[MiniGame][Startup] Critical startup step failed: " + stepName);
            }
        }

        private void TrackStartupResult(StartupStepResult result)
        {
            if (_platform == null || _platform.Telemetry == null)
                return;

            _platform.Telemetry.TrackStartupStep(
                result.StepName.ToString(),
                result.Succeeded,
                result.DurationMs,
                result.Message);
        }

        private sealed class StepOutcome
        {
            public bool Succeeded;
            public string Message;

            public static StepOutcome Success(string message)
            {
                return new StepOutcome { Succeeded = true, Message = message ?? string.Empty };
            }

            public static StepOutcome Failure(string message)
            {
                return new StepOutcome { Succeeded = false, Message = message ?? string.Empty };
            }
        }
    }
}
