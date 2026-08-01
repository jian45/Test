using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

/// <summary>
/// Addressables 运行时初始化器
/// </summary>
[DefaultExecutionOrder(-10000)]
public class AddressablesRuntimeInitializer : MonoBehaviour
{
    private static AddressablesRuntimeInitializer _instance;

    /// <summary>
    /// 全局初始化器实例。
    /// 如果场景中不存在，会自动创建一个。
    /// </summary>
    public static AddressablesRuntimeInitializer Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

#if UNITY_2023_1_OR_NEWER
            _instance = FindFirstObjectByType<AddressablesRuntimeInitializer>();
#else
            _instance = FindObjectOfType<AddressablesRuntimeInitializer>();
#endif

            if (_instance != null)
                return _instance;

            GameObject go = new GameObject("[AddressablesRuntimeInitializer]");
            _instance = go.AddComponent<AddressablesRuntimeInitializer>();
            DontDestroyOnLoad(go);

            return _instance;
        }
    }
    
    /// <summary>
    /// Addressables 是否已经初始化完成
    /// </summary>
    public static bool IsInitialized { get; private set; }
    
    /// <summary>
    /// Addressables 是否正在初始化
    /// </summary>
    public static bool IsInitializing { get; private set; }
    
    /// <summary>
    /// 最近一次初始化失败原因
    /// </summary>
    public static string LastError { get; private set; }
    
    /// <summary>
    /// 初始化成功事件
    /// </summary>
    public static event Action Initialized;
    
    /// <summary>
    /// 初始化失败事件
    /// </summary>
    public static event Action<string> InitializeFailed;
    
    [Header("初始化设置")]
    [SerializeField] private bool initializeOnAwake = true;
    [SerializeField] private bool dontDestroyOnLoad = true;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        if (initializeOnAwake)
            StartCoroutine(Initialize());
    }

    /// <summary>
    /// 初始化 Addressables
    /// </summary>
    public IEnumerator Initialize()
    {
        if (IsInitialized) yield break;

        if (IsInitializing)
        {
            while (IsInitializing)  
                yield return null;

            yield break;
        }
        
        IsInitializing = true;
        LastError = string.Empty;
        
        // 注册 Addressables 全局异常回调
        ResourceManager.ExceptionHandler = (operation, exception) =>
        {
            Debug.LogError($"[Addressables] 运行时异常：{exception}");
        };
        
        Debug.Log("[Addressables] 开始初始化...");

        // 由我们自己在初始化结束后手动 Release，便于检查初始化结果
        AsyncOperationHandle<IResourceLocator> initHandle = Addressables.InitializeAsync(false);
        yield return initHandle;

        if (initHandle.Status == AsyncOperationStatus.Succeeded)
        {
            IsInitialized = true;
            LastError = string.Empty;

            Debug.Log("[Addressables] 初始化成功。");

            Initialized?.Invoke();
        }
        else
        {
            IsInitialized = false;
            LastError = initHandle.OperationException != null
                ? initHandle.OperationException.Message
                : "Addressables 初始化失败，但 OperationException 为空。";

            Debug.LogError($"[Addressables] 初始化失败：{LastError}");

            InitializeFailed?.Invoke(LastError);
        }
        
        if (initHandle.IsValid())
            Addressables.Release(initHandle);

        IsInitializing = false;
    }
}
