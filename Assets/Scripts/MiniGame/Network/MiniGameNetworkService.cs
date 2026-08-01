using System.Threading.Tasks;
using CatCafe.MiniGame.Config;
using UnityEngine;

namespace CatCafe.MiniGame.Network
{
    public enum MiniGameNetworkResultCode
    {
        Success,
        ConfigMissing,
        Timeout,
        Failed
    }

    public sealed class MiniGameNetworkResult
    {
        public MiniGameNetworkResultCode Code;
        public string Message;
        public string Payload;

        public bool IsSuccess
        {
            get { return Code == MiniGameNetworkResultCode.Success; }
        }
    }

    public sealed class MiniGameNetworkService
    {
        private readonly MiniGameRuntimeConfig _config;

        public MiniGameNetworkService(MiniGameRuntimeConfig config)
        {
            _config = config ?? MiniGameConfigDefaults.CreateSafeDefaults();
        }

        public Task<MiniGameNetworkResult> SendPlaceholderAsync(string endpointName, string payload)
        {
            if (string.IsNullOrWhiteSpace(_config.NetworkBaseUrl))
            {
                Debug.LogWarning("[MiniGame][Network] Network base url is empty. Placeholder request skipped: " + endpointName);
                return Task.FromResult(new MiniGameNetworkResult
                {
                    Code = MiniGameNetworkResultCode.ConfigMissing,
                    Message = "Network base url is empty. No real request was sent.",
                    Payload = string.Empty
                });
            }

            Debug.Log("[MiniGame][Network] Placeholder request accepted without sending: " + endpointName);
            return Task.FromResult(new MiniGameNetworkResult
            {
                Code = MiniGameNetworkResultCode.Success,
                Message = "Placeholder network result. No real request was sent.",
                Payload = payload ?? string.Empty
            });
        }
    }
}
