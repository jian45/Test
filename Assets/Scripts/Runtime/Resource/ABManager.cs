using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// Addressables 资源加载管理器
/// </summary>
[DefaultExecutionOrder(-9999)]
public sealed class ABManager : MonoBehaviour
{
    private static ABManager _instance;

    public static ABManager Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

#if UNITY_2023_1_OR_NEWER
            _instance = FindAnyObjectByType<ABManager>();
#else
            _instance = FindObjectOfType<ABManager>();
#endif

            if (_instance != null)
                return _instance;

            GameObject go = new GameObject("[ABManager]");
            _instance = go.AddComponent<ABManager>();
            DontDestroyOnLoad(go);

            return _instance;
        }
    }

    
    /// <summary>
    /// 资源缓存记录
    /// </summary>
    private sealed class AssetCacheRecord
    {
        public string Address;
        public Type AssetType;
        public AsyncOperationHandle Handle;
        public int RefCount;
    }
    
    /// <summary>
    /// 已加载资源缓存
    /// </summary>
    private readonly Dictionary<string,AssetCacheRecord> _assetCache = 
        new Dictionary<string, AssetCacheRecord>();

    /// <summary>
    /// 通过 Addressables.InstantiateAsync 创建出来的实例缓存
    /// 释放时应使用 Addressables.ReleaseInstance
    /// </summary>
    private readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>> _instanceCache =
        new Dictionary<GameObject, AsyncOperationHandle<GameObject>>();

    /// <summary>
    /// 已加载的 Addressable 场景缓存
    /// Key：Scene Address
    /// </summary>
    private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _sceneCache =
        new Dictionary<string, AsyncOperationHandle<SceneInstance>>();


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }


    #region 初始化保障

    /// <summary>
    /// 确保 Addressables 已经进行初始化
    /// </summary>
    /// <param name="onFailed"></param>
    /// <returns></returns>
    private IEnumerator EnsureAddressablesReady(Action<string> onFailed)
    {
        if (AddressablesRuntimeInitializer.IsInitialized)
            yield break;
        
        yield return AddressablesRuntimeInitializer.Instance.Initialize();

        if (!AddressablesRuntimeInitializer.IsInitialized)
        {
            string error = string.IsNullOrEmpty(AddressablesRuntimeInitializer.LastError)
                ? "Addressables 尚未初始化，且没有明确错误信息。"
                : AddressablesRuntimeInitializer.LastError;
            
            onFailed?.Invoke(error);
        }
    }

    #endregion


    #region 通用资源加载

    /// <summary>
    /// 加载任意 Addressable 资源
    /// </summary>
    public Coroutine LoadAsset<T>(
        string address,
        Action<T> onSuccess,
        Action<string> onFailed = null) where T : Object
    {
        return StartCoroutine(LoadAssetRoutine(address, onSuccess, onFailed));
    }


    /// <summary>
    /// 加载任意 Addressable 资源的协程实现
    /// </summary>
    public IEnumerator LoadAssetRoutine<T>(
        string address,
        Action<T> onSuccess,
        Action<string> onFailed = null) where T : Object
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            onFailed?.Invoke("资源地址为空，加载失败。");
            yield break;
        }
        
        yield return EnsureAddressablesReady(onFailed);

        if (!AddressablesRuntimeInitializer.IsInitialized)
            yield break;
        
        string cacheKey = GetAssetCacheKey<T>(address);
        
        // 如果已经加载过，则复用句柄，并增加引用计数
        if (_assetCache.TryGetValue(cacheKey, out AssetCacheRecord record))
        {
            record.RefCount++;

            if (record.Handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (record.Handle.Result is T result)
                    onSuccess?.Invoke(result);
                else
                {
                    ReleaseAsset<T>(address);
                    onFailed?.Invoke($"资源类型不匹配。Address：{address}，期望类型：{typeof(T).Name}");
                }
            }
            else
            {
                ReleaseAsset<T>(address);
                onFailed?.Invoke(GetOperationError(record.Handle, $"加载缓存资源失败：{address}"));
            }
            
            yield break;
        }

        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);

        record = new AssetCacheRecord
        {
            Address = address,
            AssetType = typeof(T),
            Handle = handle,
            RefCount = 1
        };

        _assetCache.Add(cacheKey, record);

        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
            onSuccess?.Invoke(handle.Result);
        else
        {
            _assetCache.Remove(cacheKey);

            string error = GetOperationError(handle, $"加载资源失败：{address}");
            
            if (handle.IsValid())
                Addressables.Release(handle);
            
            onFailed?.Invoke(error);
        }
    }

    
    /// <summary>
    /// 释放通过 LoadAsset 加载的资源
    /// </summary>
    public bool ReleaseAsset<T>(string address) where T : Object
    {
        if (string.IsNullOrWhiteSpace(address))
            return false;
        
        string cacheKey = GetAssetCacheKey<T>(address);

        if (!_assetCache.TryGetValue(cacheKey, out AssetCacheRecord record))
            return false;
        
        record.RefCount--;
        
        if (record.RefCount > 0)
            return true;

        if (record.Handle.IsValid())
            Addressables.Release(record.Handle);

        _assetCache.Remove(cacheKey);
        return true;
    }
    

    /// <summary>
    /// 强制释放所有通过 LoadAsset 加载并缓存的资源
    /// </summary>
    public void ReleaseAllAssets()
    {
        foreach (AssetCacheRecord record in _assetCache.Values)
        {
            if (record.Handle.IsValid())
                Addressables.Release(record.Handle);
        }

        _assetCache.Clear();
    }

    #endregion


    #region Prefab 加载 实例化

    /// <summary>
    /// 加载 Prefab 原始资源，但不实例化
    /// </summary>
    public Coroutine LoadPrefabAsset(
        string address,
        Action<GameObject> onSuccess,
        Action<string> onFailed = null)
    {
        return LoadAsset(address, onSuccess, onFailed);
    }


    /// <summary>
    /// 通过 Addressables.InstantiateAsync 实例化 Prefab
    /// </summary>
    public Coroutine InstantiatePrefab(
        string address,
        Vector3 position,
        Quaternion rotation,
        Transform parent,
        Action<GameObject> onSuccess,
        Action<string> onFailed = null)
    {
        return StartCoroutine(InstantiatePrefabRoutine(
            address,
            position,
            rotation,
            parent,
            onSuccess,
            onFailed));
    }

    /// <summary>
    /// 以指定父节点异步实例化 Prefab
    /// </summary>
    public Coroutine InstantiatePrefab(
        string address,
        Transform parent,
        bool instantiateInWorldSpace,
        Action<GameObject> onSuccess,
        Action<string> onFailed = null)
    {
        return StartCoroutine(InstantiatePrefabRoutine(
            address,
            parent,
            onSuccess,
            onFailed,
            instantiateInWorldSpace));
    }

    
    /// <summary>
    /// 通过指定坐标和旋转实例化 Prefab
    /// </summary>
    private IEnumerator InstantiatePrefabRoutine(
        string address,
        Transform parent,
        Action<GameObject> onSuccess,
        Action<string> onFailed,
        bool instantiateInWorldSpace)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            onFailed?.Invoke("Prefab 地址为空，实例化失败。");
            yield break;
        }
        
        yield return EnsureAddressablesReady(onFailed);
        
        if (!AddressablesRuntimeInitializer.IsInitialized)
            yield break;

        AsyncOperationHandle<GameObject> handle =
            Addressables.InstantiateAsync(address, parent, instantiateInWorldSpace, true);
       
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject instance = handle.Result;

            if (instance != null)
                _instanceCache[instance] = handle;
            
            onSuccess?.Invoke(instance);
        }
        else
        {
            string error = GetOperationError(handle, $"实例化 Prefab 失败：{address}");

            if (handle.IsValid())
                Addressables.Release(handle);

            onFailed?.Invoke(error);
        }
    }
    
    
    private IEnumerator InstantiatePrefabRoutine(
        string address,
        Vector3 position,
        Quaternion rotation,
        Transform parent,
        Action<GameObject> onSuccess,
        Action<string> onFailed)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            onFailed?.Invoke("Prefab 地址为空，实例化失败。");
            yield break;
        }

        yield return EnsureAddressablesReady(onFailed);

        if (!AddressablesRuntimeInitializer.IsInitialized)
            yield break;

        AsyncOperationHandle<GameObject> handle =
            Addressables.InstantiateAsync(address, position, rotation, parent, true);

        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject instance = handle.Result;

            if (instance != null)
                _instanceCache[instance] = handle;

            onSuccess?.Invoke(instance);
        }
        else
        {
            string error = GetOperationError(handle, $"实例化 Prefab 失败：{address}");

            if (handle.IsValid())
                Addressables.Release(handle);

            onFailed?.Invoke(error);
        }
    }


    /// <summary>
    /// 释放通过 InstantiatePrefab 创建的实例
    /// </summary>
    public bool ReleaseInstance(GameObject instance)
    {
        if (instance == null)
            return false;

        if (_instanceCache.TryGetValue(instance, out AsyncOperationHandle<GameObject> handle))
        {
            _instanceCache.Remove(instance);

            if (handle.IsValid())
                return Addressables.ReleaseInstance(handle);

            return false;
        }
        
        return Addressables.ReleaseInstance(instance);
    }
    
    
    /// <summary>
    /// 释放所有通过 InstantiatePrefab 创建的实例
    /// </summary>
    public void ReleaseAllInstances()
    {
        List<GameObject> instances = new List<GameObject>(_instanceCache.Keys);

        for (int i = 0; i < instances.Count; i++)
        {
            ReleaseInstance(instances[i]);
        }

        _instanceCache.Clear();
    }

    #endregion


    #region Sprite / Audio / Text / ScriptableObject 快捷接口

    /// <summary>
    /// 加载 Sprite
    /// </summary>
    public Coroutine LoadSprite(
        string address,
        Action<Sprite> onSuccess,
        Action<string> onFailed = null)
    {
        return LoadAsset(address, onSuccess, onFailed);
    }
    
    
    /// <summary>
    /// 释放 Sprite
    /// </summary>
    public bool ReleaseSprite(string address)
    {
        return ReleaseAsset<Sprite>(address);
    }
    
    
    /// <summary>
    /// 加载 AudioClip
    /// </summary>
    public Coroutine LoadAudioClip(
        string address,
        Action<AudioClip> onSuccess,
        Action<string> onFailed = null)
    {
        return LoadAsset(address, onSuccess, onFailed);
    }
    
    
    /// <summary>
    /// 释放 AudioClip
    /// </summary>
    public bool ReleaseAudioClip(string address)
    {
        return ReleaseAsset<AudioClip>(address);
    }
    
    
    /// <summary>
    /// 加载 TextAsset
    /// </summary>
    public Coroutine LoadTextAsset(
        string address,
        Action<TextAsset> onSuccess,
        Action<string> onFailed = null)
    {
        return LoadAsset(address, onSuccess, onFailed);
    }
    
    
    /// <summary>
    /// 释放 TextAsset
    /// </summary>
    public bool ReleaseTextAsset(string address)
    {
        return ReleaseAsset<TextAsset>(address);
    }
    
    
    /// <summary>
    /// 加载 ScriptableObject
    /// </summary>
    public Coroutine LoadScriptableObject<T>(
        string address,
        Action<T> onSuccess,
        Action<string> onFailed = null) where T : ScriptableObject
    {
        return LoadAsset(address, onSuccess, onFailed);
    }
    
    
    /// <summary>
    /// 释放 ScriptableObject
    /// </summary>
    public bool ReleaseScriptableObject<T>(string address) where T : ScriptableObject
    {
        return ReleaseAsset<T>(address);
    }

    #endregion


    #region Scene 加载 卸载


    /// <summary>
    /// 加载 Addressable Scene
    /// </summary>
    public Coroutine LoadScene(
        string address,
        LoadSceneMode loadMode,
        Action<SceneInstance> onSuccess,
        Action<string> onFailed = null,
        bool activateOnLoad = true,
        int priority = 100)
    {
        return StartCoroutine(LoadSceneRoutine(
            address,
            loadMode,
            onSuccess,
            onFailed,
            activateOnLoad,
            priority));
    }


    /// <summary>
    /// 加载单场景模式
    /// 会替换当前普通场景
    /// </summary>
    public Coroutine LoadSceneSingle(
        string address,
        Action<SceneInstance> onSuccess,
        Action<string> onFailed = null,
        bool activateOnLoad = true,
        int priority = 100)
    {
        return LoadScene(
            address,
            LoadSceneMode.Single,
            onSuccess,
            onFailed,
            activateOnLoad,
            priority);
    }
    
    
    /// <summary>
    /// 追加加载场景
    /// </summary>
    public Coroutine LoadSceneAdditive(
        string address,
        Action<SceneInstance> onSuccess,
        Action<string> onFailed = null,
        bool activateOnLoad = true,
        int priority = 100)
    {
        return LoadScene(
            address,
            LoadSceneMode.Additive,
            onSuccess,
            onFailed,
            activateOnLoad,
            priority);
    }


    private IEnumerator LoadSceneRoutine(
        string address,
        LoadSceneMode loadMode,
        Action<SceneInstance> onSuccess,
        Action<string> onFailed,
        bool activateOnLoad,
        int priority)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            onFailed?.Invoke("Scene 地址为空，加载失败。");
            yield break;
        }

        yield return EnsureAddressablesReady(onFailed);

        if (!AddressablesRuntimeInitializer.IsInitialized)
            yield break;

        // Single 模式下，先卸载当前由 ABManager 管理的 Addressable Scene
        if (loadMode == LoadSceneMode.Single && _sceneCache.Count > 0)
        {
            yield return UnloadAllScenesRoutine(null, null);
        }

        if (_sceneCache.ContainsKey(address))
        {
            onFailed?.Invoke(($"Scene 已经加载，不能重复加载：{address}"));
            yield break;
        }
        
        AsyncOperationHandle<SceneInstance> handle =
            Addressables.LoadSceneAsync(address, loadMode, activateOnLoad, priority);
        
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _sceneCache[address] = handle;
            onSuccess?.Invoke(handle.Result);
        }
        else
        {
            string error = GetOperationError(handle, $"加载 Scene 失败：{address}");
            
            if (handle.IsValid())
                Addressables.Release(handle);

            onFailed?.Invoke(error);
        }
    }

    
    /// <summary>
    /// 卸载指定 Addressable Scene
    /// </summary>
    public Coroutine UnloadScene(
        string address,
        Action onSuccess = null,
        Action<string> onFailed = null)
    {
        return StartCoroutine(UnloadSceneRoutine(address, onSuccess, onFailed));
    }
    

    private IEnumerator UnloadSceneRoutine(
        string address,
        Action onSuccess,
        Action<string> onFailed)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            onFailed?.Invoke("Scene 地址为空，卸载失败。");
            yield break;
        }

        if (!_sceneCache.TryGetValue(address, out AsyncOperationHandle<SceneInstance> handle))
        {
            onFailed?.Invoke($"Scene 没有被 ABManager 记录，无法卸载：{address}");
            yield break;
        }
        
        if (!handle.IsValid())
        {
            _sceneCache.Remove(address);

            onFailed?.Invoke(
                $"Scene 卸载失败：缓存中的 Scene Handle 已经无效。\n" +
                $"Address: {address}\n" +
                $"可能原因：该场景已经被卸载、handle 已经被 Release、Single Scene 切换时被自动清理、或重复卸载。");

            yield break;
        }

        // 如果异步操作尚未完成，也不能直接卸载
        if (!handle.IsDone)
        {
            onFailed?.Invoke(
                $"Scene 尚未加载完成，不能卸载。\n" +
                $"Address: {address}");

            yield break;
        }

        AsyncOperationHandle<SceneInstance> unloadHandle = default;

        try
        {
            unloadHandle = Addressables.UnloadSceneAsync(handle, false);
        }
        catch (Exception e)
        {
            _sceneCache.Remove(address);

            onFailed?.Invoke(
                $"Addressables.UnloadSceneAsync 同步异常。\n" +
                $"Address: {address}\n" +
                $"Exception: {e.GetType().Name}\n" +
                $"Message: {e.Message}");

            Debug.LogException(e);
            yield break;
        }

        if (!unloadHandle.IsValid())
        {
            _sceneCache.Remove(address);
            onFailed?.Invoke($"Scene 卸载失败，Unload Handle 无效。Address: {address}");
            yield break;
        }

        yield return unloadHandle;

        if (unloadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            _sceneCache.Remove(address);
            onSuccess?.Invoke();
        }
        else
        {
            string error = unloadHandle.OperationException != null
                ? unloadHandle.OperationException.Message
                : "未知错误";

            onFailed?.Invoke($"卸载 Scene 失败：{address}，错误：{error}");
        }

        // 手动释放卸载操作句柄
        if (unloadHandle.IsValid())
            Addressables.Release(unloadHandle);
    }


    /// <summary>
    /// 卸载所有由 ABManager 加载的 Addressable Scene
    /// </summary>
    public Coroutine UnloadAllScenes(Action onSuccess, Action<string> onFailed = null)
    {
        return StartCoroutine(UnloadAllScenesRoutine(onSuccess, onFailed));
    }


    private IEnumerator UnloadAllScenesRoutine(
        Action onSuccess,
        Action<string> onFailed)
    {
        List<string> sceneAddresses = new List<string>(_sceneCache.Keys);

        for (int i = 0; i < sceneAddresses.Count; i++)
        {
            string address = sceneAddresses[i];

            if (!_sceneCache.TryGetValue(address, out AsyncOperationHandle<SceneInstance> handle))
                continue;

            if (!handle.IsValid())
            {
                _sceneCache.Remove(address);

                Debug.LogWarning(
                    $"[ABManager] 跳过无效 Scene Handle，并移除缓存记录。\n" +
                    $"Address: {address}");

                continue;
            }

            if (!handle.IsDone)
            {
                onFailed?.Invoke(
                    $"Scene 尚未加载完成，不能卸载。\n" +
                    $"Address: {address}");

                yield break;
            }

            AsyncOperationHandle<SceneInstance> unloadHandle = default;

            try
            {
                unloadHandle = Addressables.UnloadSceneAsync(handle, false);
            }
            catch (Exception e)
            {
                _sceneCache.Remove(address);

                onFailed?.Invoke(
                    $"Addressables.UnloadSceneAsync 同步异常。\n" +
                    $"Address: {address}\n" +
                    $"Exception: {e.GetType().Name}\n" +
                    $"Message: {e.Message}");

                Debug.LogException(e);
                yield break;
            }

            if (!unloadHandle.IsValid())
            {
                _sceneCache.Remove(address);
                onFailed?.Invoke($"Scene 卸载失败，Unload Handle 无效。Address: {address}");
                yield break;
            }

            yield return unloadHandle;

            if (unloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _sceneCache.Remove(address);
            }
            else
            {
                string error = unloadHandle.OperationException != null
                    ? unloadHandle.OperationException.Message
                    : "未知错误";

                onFailed?.Invoke($"卸载 Scene 失败：{address}，错误：{error}");

                if (unloadHandle.IsValid())
                    Addressables.Release(unloadHandle);

                yield break;
            }

            if (unloadHandle.IsValid())
                Addressables.Release(unloadHandle);
        }

        onSuccess?.Invoke();
    }

    #endregion
    

    #region 工具方法

    private static string GetAssetCacheKey<T>(string address) where T : Object
    {
        return $"{typeof(T).Name}::{address})";
    }


    private static string GetOperationError(AsyncOperationHandle handle, string defaultMessage)
    {
        if(handle.OperationException != null)
            return $"{defaultMessage},异常:{handle.OperationException.Message}";
        
        return defaultMessage;
    }
    
    
    private static string GetOperationError<T>(AsyncOperationHandle<T> handle, string defaultMessage)
    {
        if (handle.OperationException != null)
            return $"{defaultMessage}，异常：{handle.OperationException.Message}";

        return defaultMessage;
    }


    /// <summary>
    /// 释放所有资源
    /// </summary>
    public void ReleaseAll()
    {
        ReleaseAllInstances();
        ReleaseAllAssets();
    }
    
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            ReleaseAll();
            _instance = null;
        }
    }

    #endregion
}
