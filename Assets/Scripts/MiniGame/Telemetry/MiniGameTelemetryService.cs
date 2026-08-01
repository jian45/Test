using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CatCafe.MiniGame.Config;
using CatCafe.MiniGame.Contracts.Telemetry;
using UnityEngine;

namespace CatCafe.MiniGame.Telemetry
{
    public sealed class MiniGameTelemetryService : IMiniGameTelemetry
    {
        private readonly List<string> _localQueue = new List<string>();
        private MiniGameRuntimeConfig _config;

        public bool IsInitialized { get; private set; }

        public Task<bool> InitializeAsync(MiniGameRuntimeConfig config)
        {
            _config = config ?? MiniGameConfigDefaults.CreateSafeDefaults();

            if (_config.SimulateTelemetryFailure)
            {
                IsInitialized = false;
                Debug.LogWarning("[MiniGame][Telemetry] Mock telemetry initialization failed by config.");
                return Task.FromResult(false);
            }

            IsInitialized = true;
            Debug.Log("[MiniGame][Telemetry] Mock telemetry initialized.");
            return Task.FromResult(true);
        }

        public bool TrackEvent(string eventName, IDictionary<string, string> fields)
        {
            if (_config != null && _config.SimulateTelemetryFailure)
                return false;

            string line = FormatEvent(eventName, fields);
            _localQueue.Add(line);
            Debug.Log(line);
            return true;
        }

        public bool TrackStartupStep(string stepName, bool success, long durationMs, string message)
        {
            return TrackEvent(
                "startup_step",
                new Dictionary<string, string>
                {
                    { "step", stepName ?? string.Empty },
                    { "success", success ? "true" : "false" },
                    { "durationMs", durationMs.ToString() },
                    { "message", message ?? string.Empty }
                });
        }

        public bool TrackError(string source, string message)
        {
            return TrackEvent(
                "error",
                new Dictionary<string, string>
                {
                    { "source", source ?? string.Empty },
                    { "message", message ?? string.Empty }
                });
        }

        private static string FormatEvent(string eventName, IDictionary<string, string> fields)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[MiniGame][Telemetry] ");
            builder.Append(eventName ?? string.Empty);

            if (fields != null)
            {
                foreach (KeyValuePair<string, string> pair in fields)
                {
                    builder.Append(" ");
                    builder.Append(pair.Key);
                    builder.Append("=");
                    builder.Append(pair.Value);
                }
            }

            return builder.ToString();
        }
    }
}
