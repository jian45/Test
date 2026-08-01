using System;

namespace CatCafe.MiniGame.App.Bootstrap
{
    [Serializable]
    public sealed class StartupStepResult
    {
        public StartupStepName StepName;
        public bool Succeeded;
        public long DurationMs;
        public string Message;

        public static StartupStepResult Success(StartupStepName stepName, long durationMs, string message)
        {
            return new StartupStepResult
            {
                StepName = stepName,
                Succeeded = true,
                DurationMs = durationMs,
                Message = message ?? string.Empty
            };
        }

        public static StartupStepResult Failure(StartupStepName stepName, long durationMs, string message)
        {
            return new StartupStepResult
            {
                StepName = stepName,
                Succeeded = false,
                DurationMs = durationMs,
                Message = message ?? string.Empty
            };
        }
    }
}
