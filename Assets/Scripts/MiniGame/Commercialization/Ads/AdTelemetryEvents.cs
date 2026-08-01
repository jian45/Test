namespace CatCafe.MiniGame.Commercialization.Ads
{
    public static class AdTelemetryEvents
    {
        public const string Request = "ad_request";
        public const string PolicyBlocked = "ad_policy_blocked";
        public const string LoadStart = "ad_load_start";
        public const string LoadSuccess = "ad_load_success";
        public const string LoadFailed = "ad_load_failed";
        public const string ShowStart = "ad_show_start";
        public const string ShowFailed = "ad_show_failed";
        public const string CloseCompleted = "ad_close_completed";
        public const string CloseSkipped = "ad_close_skipped";
        public const string RewardGranted = "ad_reward_granted";
        public const string RewardFailed = "ad_reward_failed";
    }
}
