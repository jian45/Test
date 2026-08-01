using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatCafe.MiniGame.Config;
using CatCafe.MiniGame.Contracts.Resource;
using CatCafe.MiniGame.Contracts.Telemetry;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

namespace CatCafe.MiniGame.Resource
{
    public sealed class AddressablesMiniGameResourceService : IMiniGameResourceService
    {
        private readonly Dictionary<string, AsyncOperationHandle> _loadedHandles =
            new Dictionary<string, AsyncOperationHandle>();

        private MiniGameRuntimeConfig _config;
        private IMiniGameTelemetry _telemetry;

        public bool IsInitialized { get; private set; }
        public string LastError { get; private set; }

        public async Task<bool> InitializeAsync(MiniGameRuntimeConfig config, IMiniGameTelemetry telemetry)
        {
            _config = config ?? MiniGameConfigDefaults.CreateSafeDefaults();
            _telemetry = telemetry;
            LastError = string.Empty;

            try
            {
                AsyncOperationHandle<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator> handle =
                    Addressables.InitializeAsync(false);
                await WaitForCompletionAsync(handle);

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    IsInitialized = true;
                    Debug.Log("[MiniGame][Resource] Addressables initialized for MiniGame resource service.");
                    Track("resource_initialize", "success", string.Empty);
                }
                else
                {
                    LastError = GetOperationError(handle, "Addressables initialization failed.");
                    IsInitialized = false;
                    Debug.LogWarning("[MiniGame][Resource] " + LastError);
                    _telemetry?.TrackError("AddressablesInitialize", LastError);
                }

                if (handle.IsValid())
                    Addressables.Release(handle);

                return IsInitialized;
            }
            catch (Exception exception)
            {
                LastError = exception.Message;
                IsInitialized = false;
                Debug.LogException(exception);
                _telemetry?.TrackError("AddressablesInitialize", LastError);
                return false;
            }
        }

        public async Task<bool> WarmupFirstPlayResourcesAsync()
        {
            if (!IsInitialized)
            {
                LastError = "Addressables resource service is not initialized.";
                _telemetry?.TrackError("AddressablesWarmup", LastError);
                return false;
            }

            if (_config == null || _config.FirstPlayWarmupLabels == null || _config.FirstPlayWarmupLabels.Count == 0)
            {
                Debug.Log("[MiniGame][Resource] No Addressables warmup labels configured.");
                Track("resource_warmup", "empty", string.Empty);
                return true;
            }

            bool allSucceeded = true;
            for (int i = 0; i < _config.FirstPlayWarmupLabels.Count; i++)
            {
                string label = _config.FirstPlayWarmupLabels[i];
                if (string.IsNullOrWhiteSpace(label))
                    continue;

                bool warmed = await WarmupLabelAsync(label);
                allSucceeded &= warmed;
            }

            return allSucceeded;
        }

        public async Task<T> LoadAssetAsync<T>(string address) where T : Object
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                LastError = "Addressables address is empty.";
                _telemetry?.TrackError("AddressablesLoadAsset", LastError);
                return null;
            }

            if (!IsInitialized)
            {
                bool initialized = await InitializeAsync(_config, _telemetry);
                if (!initialized)
                    return null;
            }

            if (_loadedHandles.TryGetValue(address, out AsyncOperationHandle cachedHandle) &&
                cachedHandle.IsValid() &&
                cachedHandle.Result is T cachedAsset)
            {
                return cachedAsset;
            }

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            await WaitForCompletionAsync(handle);

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _loadedHandles[address] = handle;
                Track("resource_load", "success", address);
                return handle.Result;
            }

            LastError = GetOperationError(handle, "Addressables asset load failed. Address: " + address);
            Debug.LogWarning("[MiniGame][Resource] " + LastError);
            _telemetry?.TrackError("AddressablesLoadAsset", LastError);

            if (handle.IsValid())
                Addressables.Release(handle);

            return null;
        }

        public void ReleaseAsset(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return;

            if (!_loadedHandles.TryGetValue(address, out AsyncOperationHandle handle))
                return;

            if (handle.IsValid())
                Addressables.Release(handle);

            _loadedHandles.Remove(address);
            Track("resource_release", "asset", address);
        }

        public void ReleaseAll()
        {
            foreach (KeyValuePair<string, AsyncOperationHandle> pair in _loadedHandles)
            {
                AsyncOperationHandle handle = pair.Value;
                if (handle.IsValid())
                    Addressables.Release(handle);
            }

            _loadedHandles.Clear();
            Track("resource_release", "all", string.Empty);
        }

        private async Task<bool> WarmupLabelAsync(string label)
        {
            AsyncOperationHandle<IList<IResourceLocation>> locationsHandle =
                Addressables.LoadResourceLocationsAsync(label);
            await WaitForCompletionAsync(locationsHandle);

            if (locationsHandle.Status != AsyncOperationStatus.Succeeded ||
                locationsHandle.Result == null ||
                locationsHandle.Result.Count == 0)
            {
                LastError = GetOperationError(locationsHandle, "Addressables warmup label has no locations. Label: " + label);
                Debug.LogWarning("[MiniGame][Resource] " + LastError);
                _telemetry?.TrackError("AddressablesWarmupLabel", LastError);

                if (locationsHandle.IsValid())
                    Addressables.Release(locationsHandle);

                return false;
            }

            AsyncOperationHandle<IList<Object>> assetsHandle =
                Addressables.LoadAssetsAsync<Object>(locationsHandle.Result, null);
            await WaitForCompletionAsync(assetsHandle);

            if (locationsHandle.IsValid())
                Addressables.Release(locationsHandle);

            if (assetsHandle.Status == AsyncOperationStatus.Succeeded)
            {
                string key = "label:" + label;
                _loadedHandles[key] = assetsHandle;
                Track("resource_warmup", "success", label);
                return true;
            }

            LastError = GetOperationError(assetsHandle, "Addressables warmup failed. Label: " + label);
            Debug.LogWarning("[MiniGame][Resource] " + LastError);
            _telemetry?.TrackError("AddressablesWarmupLabel", LastError);

            if (assetsHandle.IsValid())
                Addressables.Release(assetsHandle);

            return false;
        }

        private static Task<AsyncOperationHandle<T>> WaitForCompletionAsync<T>(AsyncOperationHandle<T> handle)
        {
            if (handle.IsDone)
                return Task.FromResult(handle);

            TaskCompletionSource<AsyncOperationHandle<T>> completion =
                new TaskCompletionSource<AsyncOperationHandle<T>>();
            handle.Completed += completed => completion.TrySetResult(completed);
            return completion.Task;
        }

        private static string GetOperationError<T>(AsyncOperationHandle<T> handle, string defaultMessage)
        {
            if (handle.OperationException != null)
                return defaultMessage + " Exception: " + handle.OperationException.Message;

            return defaultMessage;
        }

        private void Track(string eventName, string status, string target)
        {
            if (_telemetry == null)
                return;

            _telemetry.TrackEvent(
                eventName,
                new Dictionary<string, string>
                {
                    { "status", status ?? string.Empty },
                    { "target", target ?? string.Empty }
                });
        }
    }
}
