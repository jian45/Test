using System;

namespace CatCafe.MiniGame.Contracts.Commercialization
{
    public enum RewardedAdResultCode
    {
        Granted,
        Skipped,
        LoadFailed,
        ShowFailed,
        Unsupported,
        Busy,
        ConfigMissing,
        PolicyBlocked,
        Loaded
    }

    [Serializable]
    public sealed class RewardedAdResult
    {
        public RewardedAdResultCode Code;
        public string PlacementId;
        public string RewardId;
        public string RewardRequestId;
        public string Message;
        public bool RewardSettled;

        public bool IsGranted
        {
            get { return Code == RewardedAdResultCode.Granted && RewardSettled; }
        }

        public static RewardedAdResult Create(
            RewardedAdResultCode code,
            string placementId,
            string rewardId,
            string rewardRequestId,
            string message,
            bool rewardSettled = false)
        {
            return new RewardedAdResult
            {
                Code = code,
                PlacementId = placementId ?? string.Empty,
                RewardId = rewardId ?? string.Empty,
                RewardRequestId = rewardRequestId ?? string.Empty,
                Message = message ?? string.Empty,
                RewardSettled = rewardSettled
            };
        }

        public static RewardedAdResult FromRequest(
            RewardedAdRequest request,
            RewardedAdResultCode code,
            string message,
            bool rewardSettled = false)
        {
            if (request == null)
            {
                return Create(code, string.Empty, string.Empty, string.Empty, message, rewardSettled);
            }

            return Create(
                code,
                request.PlacementId,
                request.RewardId,
                request.RewardRequestId,
                message,
                rewardSettled);
        }
    }
}
