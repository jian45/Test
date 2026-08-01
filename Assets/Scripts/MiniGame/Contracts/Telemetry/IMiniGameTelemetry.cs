using System.Collections.Generic;
using System.Threading.Tasks;
using CatCafe.MiniGame.Config;

namespace CatCafe.MiniGame.Contracts.Telemetry
{
    public interface IMiniGameTelemetry
    {
        bool IsInitialized { get; }

        Task<bool> InitializeAsync(MiniGameRuntimeConfig config);
        bool TrackEvent(string eventName, IDictionary<string, string> fields);
        bool TrackStartupStep(string stepName, bool success, long durationMs, string message);
        bool TrackError(string source, string message);
    }
}
