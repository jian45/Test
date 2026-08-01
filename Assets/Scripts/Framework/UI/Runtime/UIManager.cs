using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClientFramework.UI
{
    public sealed class UIManager : MonoBehaviour
    {
        private sealed class PanelRecord
        {
            public UIBase Panel;
            public UIPanelInfo Info;
        }

        private sealed class LoadingRequest
        {
            public UIPanelInfo Info;
            public readonly List<Action<UIBase>> Loaded =
                new List<Action<UIBase>>();
            public readonly List<Action<string>> Failed =
                new List<Action<string>>();
            public bool CloseRequested;
        }

        public static UIManager Instance { get; private set; }

        [SerializeField] private UIRoot uiRoot;

        private readonly Dictionary<Type, PanelRecord> panels =
            new Dictionary<Type, PanelRecord>();
        private readonly Dictionary<Type, LoadingRequest> loading =
            new Dictionary<Type, LoadingRequest>();
        private readonly List<UIBase> refreshBuffer =
            new List<UIBase>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void OpenPanel<T>(
            UIPanelInfo info,
            Action<T> onLoaded,
            Action<string> onFailed = null)
            where T : UIBase
        {
            Type type = typeof(T);

            if (string.IsNullOrWhiteSpace(info.Address))
            {
                InvokeFailed(onFailed, type.Name + " address is empty.");
                return;
            }

            if (uiRoot == null)
            {
                InvokeFailed(onFailed, "UIRoot is missing.");
                return;
            }

            if (panels.TryGetValue(type, out PanelRecord record))
            {
                if (!string.Equals(
                        record.Info.Address,
                        info.Address,
                        StringComparison.Ordinal))
                {
                    InvokeFailed(
                        onFailed,
                        type.Name + " is already opened with another address.");
                    return;
                }

                T panel = record.Panel as T;
                if (panel == null)
                {
                    InvokeFailed(
                        onFailed,
                        type.Name + " component type mismatch.");
                    return;
                }

                InvokeLoaded(onLoaded, panel);

                if (panels.TryGetValue(type, out PanelRecord current) &&
                    current.Panel == panel)
                {
                    panel.InternalShow();
                }

                return;
            }

            if (loading.TryGetValue(type, out LoadingRequest existingRequest))
            {
                if (!string.Equals(
                        existingRequest.Info.Address,
                        info.Address,
                        StringComparison.Ordinal))
                {
                    InvokeFailed(
                        onFailed,
                        type.Name + " is loading with another address.");
                    return;
                }

                AddCallbacks(existingRequest, onLoaded, onFailed);
                return;
            }

            Transform parent = uiRoot.GetLayer(info.Layer);
            if (parent == null)
            {
                InvokeFailed(
                    onFailed,
                    "UI layer root is missing: " + info.Layer + ".");
                return;
            }

            LoadingRequest request = new LoadingRequest { Info = info };
            AddCallbacks(request, onLoaded, onFailed);
            loading.Add(type, request);

            ABManager.Instance.InstantiatePrefab(
                info.Address,
                parent,
                false,
                instance => HandleLoaded<T>(type, request, instance),
                error => HandleFailed(type, request, error));
        }

        public void ClosePanel<T>() where T : UIBase
        {
            Type type = typeof(T);

            if (loading.TryGetValue(type, out LoadingRequest request))
            {
                request.CloseRequested = true;
                return;
            }

            if (!panels.TryGetValue(type, out PanelRecord record))
                return;

            record.Panel.InternalHide();

            if (record.Info.CacheMode == UICacheMode.DestroyOnClose)
                ReleaseRecord(type, record);
        }

        public void ReleasePanel<T>() where T : UIBase
        {
            Type type = typeof(T);

            if (loading.TryGetValue(type, out LoadingRequest request))
            {
                request.CloseRequested = true;
                return;
            }

            if (panels.TryGetValue(type, out PanelRecord record))
                ReleaseRecord(type, record);
        }

        public T GetPanel<T>() where T : UIBase
        {
            return panels.TryGetValue(
                    typeof(T),
                    out PanelRecord record)
                ? record.Panel as T
                : null;
        }

        public bool IsOpened<T>() where T : UIBase
        {
            T panel = GetPanel<T>();
            return panel != null && panel.gameObject.activeSelf;
        }

        public void RefreshAllOpenedUI()
        {
            refreshBuffer.Clear();

            foreach (PanelRecord record in panels.Values)
            {
                UIBase panel = record.Panel;
                if (panel != null && panel.gameObject.activeInHierarchy)
                    refreshBuffer.Add(panel);
            }

            for (int i = 0; i < refreshBuffer.Count; i++)
            {
                UIBase panel = refreshBuffer[i];
                if (panel == null || !panel.gameObject.activeInHierarchy)
                    continue;

                try
                {
                    panel.UIUpdate();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }

            refreshBuffer.Clear();
        }

        public void ReleaseAll()
        {
            List<Type> types = new List<Type>(panels.Keys);

            for (int i = 0; i < types.Count; i++)
            {
                if (panels.TryGetValue(types[i], out PanelRecord record))
                    ReleaseRecord(types[i], record);
            }

            foreach (LoadingRequest request in loading.Values)
                request.CloseRequested = true;
        }

        private void HandleLoaded<T>(
            Type type,
            LoadingRequest request,
            GameObject instance)
            where T : UIBase
        {
            if (!loading.TryGetValue(type, out LoadingRequest currentRequest) ||
                currentRequest != request)
            {
                if (instance != null)
                    ABManager.Instance.ReleaseInstance(instance);
                return;
            }

            loading.Remove(type);

            if (instance == null)
            {
                InvokeFailed(
                    request,
                    type.Name + " loaded a null instance.");
                return;
            }

            if (request.CloseRequested)
            {
                ABManager.Instance.ReleaseInstance(instance);
                InvokeFailed(request, type.Name + " load canceled.");
                return;
            }

            T panel = instance.GetComponent<T>();
            if (panel == null)
            {
                ABManager.Instance.ReleaseInstance(instance);
                InvokeFailed(
                    request,
                    instance.name + " does not contain " + type.Name + ".");
                return;
            }

            instance.SetActive(false);
            panel.InternalCreate();

            panels[type] = new PanelRecord
            {
                Panel = panel,
                Info = request.Info
            };

            InvokeLoaded(request, panel);

            if (panels.TryGetValue(type, out PanelRecord current) &&
                current.Panel == panel)
            {
                panel.InternalShow();
            }
        }

        private void HandleFailed(
            Type type,
            LoadingRequest request,
            string error)
        {
            if (!loading.TryGetValue(type, out LoadingRequest currentRequest) ||
                currentRequest != request)
            {
                return;
            }

            loading.Remove(type);
            InvokeFailed(request, error);
        }

        private void ReleaseRecord(Type type, PanelRecord record)
        {
            panels.Remove(type);

            if (record.Panel == null)
            {
                Debug.LogError(
                    "[UI] Panel instance is missing before release: " +
                    type.Name);
                return;
            }

            record.Panel.InternalRelease();
            // 修改人：黎永健 — 停止运行时 ABManager 可能已销毁，跳过释放避免 new 空壳
            if (_isShuttingDown)
                return;

            if (!ABManager.Instance.ReleaseInstance(record.Panel.gameObject))
            {
                if (!ABManager.Instance.ReleaseInstance(record.Panel.gameObject))
                {
                    Debug.LogError(
                    "[UI] Addressable instance is not owned by ABManager: " +
                      type.Name);
                }
            }
        }

        private static void AddCallbacks<T>(
            LoadingRequest request,
            Action<T> onLoaded,
            Action<string> onFailed)
            where T : UIBase
        {
            if (onLoaded != null)
                request.Loaded.Add(panel => onLoaded((T)panel));

            if (onFailed != null)
                request.Failed.Add(onFailed);
        }

        private static void InvokeLoaded<T>(
            Action<T> callback,
            T panel)
            where T : UIBase
        {
            if (callback == null)
                return;

            try
            {
                callback(panel);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void InvokeLoaded(
            LoadingRequest request,
            UIBase panel)
        {
            for (int i = 0; i < request.Loaded.Count; i++)
            {
                try
                {
                    request.Loaded[i](panel);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        private static void InvokeFailed(
            Action<string> callback,
            string error)
        {
            if (callback == null)
                return;

            try
            {
                callback(error);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void InvokeFailed(
            LoadingRequest request,
            string error)
        {
            for (int i = 0; i < request.Failed.Count; i++)
            {
                try
                {
                    request.Failed[i](error);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        private bool _isShuttingDown;

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            // 停止运行时 ABManager 可能先于 UIManager 被销毁，
            // 此时 ABManager.Instance getter 会重建一个空壳导致 ReleaseAll 报错。
            //修改人：黎永健
            _isShuttingDown = true;
            ReleaseAll();
            Instance = null;
        }
    }
}
