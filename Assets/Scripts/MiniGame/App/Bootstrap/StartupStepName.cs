namespace CatCafe.MiniGame.App.Bootstrap
{
    public enum StartupStepName
    {
        DetectEnvironment,
        CreatePlatform,
        InitializePlatform,
        LoadLocalConfig,
        InitializeTelemetry,
        InitializeResourceService,
        WarmupFirstPlayResources,
        InitializeAds,
        ReportGameStart,
        EnterReadyState
    }
}
