#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;


/// <summary>
/// Addressables 自动短地址工具
/// </summary>
public static class AddressablesAutoAddressConfig
{
    private const string RootFolderKey = "AddressablesAutoAddress.RootFolder";
    private const string AutoProcessKey = "AddressablesAutoAddress.AutoProcess";
    private const string IncludeFolderEntriesKey = "AddressablesAutoAddress.IncludeFolderEntries";

    public const string DefaultRootFolder = "Assets/GameRes/AddressableAssets";

    /// <summary>
    /// Addressables 自动处理的资源根目录
    /// </summary>
    public static string RootFolder
    {
        get
        {
            string value = EditorPrefs.GetString(RootFolderKey,DefaultRootFolder);
            return NormalizePath(value).TrimEnd('/');
        }
        set
        {
            EditorPrefs.SetString(RootFolderKey,NormalizePath(value).TrimEnd('/'));
        }
    }


    /// <summary>
    /// 是否启用导入资源时自动加入 Addressables
    /// </summary>
    public static bool AutoProcessOnImport
    {
        get => EditorPrefs.GetBool(AutoProcessKey, false);
        set => EditorPrefs.SetBool(AutoProcessKey, value);
    }


    /// <summary>
    /// 是否把文件夹本身也作为 Addressable Entry
    /// </summary>
    public static bool IncludeFolderEntries
    {
        get => EditorPrefs.GetBool(IncludeFolderEntriesKey, false);
        set => EditorPrefs.SetBool(IncludeFolderEntriesKey, value);
    }


    public static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
        
        return path.Replace('\\', '/');
    }
}


/// <summary>
/// Addressables 自动短地址处理工具类
/// </summary>
public static class AddressablesAutoAddressUtility
{
    /// <summary>
    /// 不建议自动加入 Addressables 的文件后缀
    /// </summary>
    private static readonly HashSet<string> ExcludedExtensions = new HashSet<string>
    {
        ".cs",
        ".asmdef",
        ".asmref",
        ".dll",
        ".meta",
        ".mdb",
        ".pdb",
        ".DS_Store"
    };


    /// <summary>
    /// 处理报告
    /// </summary>
    public sealed class ProcessReport
    {
        public int ScannedCount;
        public int AddedCount;
        public int RenamedCount;
        public int RemovedCount;
        public int SkippedCount;
        public int WarningCount;
        
        public readonly List<string> Messages = new List<string>();

        public void AddMessage(string message)
        {
            Messages.Add(message);
        }
        
        public void AddWarning(string message)
        {
            WarningCount++;
            Messages.Add("[Warning] " + message);
        }
        
        public void Log(string title)
        {
            Debug.Log(
                $"{title}\n" +
                $"扫描数量：{ScannedCount}\n" +
                $"新增数量：{AddedCount}\n" +
                $"改名数量：{RenamedCount}\n" +
                $"移除数量：{RemovedCount}\n" +
                $"跳过数量：{SkippedCount}\n" +
                $"警告数量：{WarningCount}");

            for (int i = 0; i < Messages.Count; i++)
            {
                Debug.Log(Messages[i]);
            }
        }
    }


    /// <summary>
    /// 获取 Addressables Settings
    /// </summary>
    public static AddressableAssetSettings GetSettings()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        
        if (settings == null)
        {
            Debug.LogError(
                "没有找到 AddressableAssetSettings。\n" +
                "请先打开 Window > Asset Management > Addressables > Groups，并点击 Create Addressables Settings。");
        }

        return settings;
    }


    /// <summary>
    /// 获取默认 Group
    /// </summary>
    public static AddressableAssetGroup GetDefaultGroup(AddressableAssetSettings settings)
    {
        if (settings == null)
            return null;
        
        if (settings.DefaultGroup != null)
            return settings.DefaultGroup;
        
        AddressableAssetGroup localGroup = settings.FindGroup("Local Group (Default)");
        if (localGroup != null)
            return localGroup;
        
        foreach (AddressableAssetGroup group in settings.groups)
        {
            if (group != null)
                return group;
        }
        
        return null;
    }


    /// <summary>
    /// 判断路径是否位于 Addressables 自动处理根目录下
    /// </summary>
    public static bool IsUnderRootFolder(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
            return false;
        
        string path = NormalizePath(assetPath);
        string root = NormalizePath(AddressablesAutoAddressConfig.RootFolder).TrimEnd('/');
        
        return path == root || path.StartsWith(root + "/", StringComparison.Ordinal);
    }


    /// <summary>
    /// 将资源路径转换为短 Address
    /// </summary>
    public static string ConvertAssetPathToShortAddress(string assetPath)
    {
        assetPath = NormalizePath(assetPath);
        
        string root = NormalizePath(AddressablesAutoAddressConfig.RootFolder).TrimEnd('/');

        if (!assetPath.StartsWith(root + "/", StringComparison.Ordinal))
            return string.Empty;

        string relativePath = assetPath.Substring(root.Length + 1);

        if (string.IsNullOrEmpty(relativePath))
            return string.Empty;
        
        bool isFolder = AssetDatabase.IsValidFolder(assetPath);

        if (isFolder)
            return relativePath.Trim('/');
        
        string directory = Path.GetDirectoryName(relativePath);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(relativePath);
        
        if (string.IsNullOrEmpty(directory))
            return fileNameWithoutExtension;
        
        directory = NormalizePath(directory);

        return $"{directory}/{fileNameWithoutExtension}";
    }


    /// <summary>
    /// 判断一个资源是否可以作为 Addressable 资源
    /// </summary>
    public static bool IsAddressableCandidate(string assetPath, bool includeFolderEntry)
    {
        if (string.IsNullOrEmpty(assetPath))
            return false;
        
        assetPath = NormalizePath(assetPath);

        if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
            return false;
        
        if (!IsUnderRootFolder(assetPath))
            return false;
        
        string root = NormalizePath(AddressablesAutoAddressConfig.RootFolder).TrimEnd('/');
        
        if (assetPath == root)
            return false;
        
        if (AssetDatabase.IsValidFolder(assetPath))
            return includeFolderEntry;
        
        string extension = Path.GetExtension(assetPath);
        
        if (!string.IsNullOrEmpty(extension) && ExcludedExtensions.Contains(extension))
            return false;
        
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
        if (asset == null)
            return false;

        return true;
    }


    /// <summary>
    /// 把指定资源加入 Addressables，并设置短 Address
    /// </summary>
    public static bool AddOrNormalizeAsset(
        AddressableAssetSettings settings,
        AddressableAssetGroup targetGroup,
        string assetPath,
        bool includeFolderEntry,
        bool dryRun,
        ProcessReport report)
    {
        report.ScannedCount++;
        
        assetPath = NormalizePath(assetPath);
        
        if (!IsAddressableCandidate(assetPath, includeFolderEntry))
        {
            report.SkippedCount++;
            return false;
        }
        
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        
        if (string.IsNullOrEmpty(guid))
        {
            report.SkippedCount++;
            report.AddWarning($"无法获取 GUID：{assetPath}");
            return false;
        }
        
        string shortAddress = ConvertAssetPathToShortAddress(assetPath);
        
        if (string.IsNullOrEmpty(shortAddress))
        {
            report.SkippedCount++;
            report.AddWarning($"无法生成短 Address：{assetPath}");
            return false;
        }
        
        AddressableAssetEntry entry = settings.FindAssetEntry(guid);

        bool changed = false;

        if (entry == null)
        {
            if (targetGroup == null)
            {
                report.SkippedCount++;
                report.AddWarning($"没有可用的 Addressables Group，无法加入：{assetPath}");
                return false;
            }

            if (!dryRun)
            {
                entry = settings.CreateOrMoveEntry(guid, targetGroup, false, true);
                entry.SetAddress(shortAddress);
            }
            
            report.AddedCount++;
            report.AddMessage($"新增 Addressable：{assetPath}  →  {shortAddress}");
            changed = true;
        }
        else
        {
            if (entry.address != shortAddress)
            {
                if (!dryRun)
                {
                    entry.SetAddress(shortAddress);
                }

                report.RenamedCount++;
                report.AddMessage($"修正 Address：{entry.address}  →  {shortAddress}");
                changed = true;
            }
        }
        
        return changed;
    }

    
    /// <summary>
    /// 扫描根目录下所有资源，自动加入 Addressables，并生成短地址
    /// </summary>
    public static ProcessReport ScanRootFolderAndAddMissingAssets(bool includeFolderEntry, bool dryRun)
    {
        ProcessReport report = new ProcessReport();
            
        AddressableAssetSettings settings = GetSettings();
        if (settings == null)
            return report;
        
        AddressableAssetGroup defaultGroup = GetDefaultGroup(settings);

        string root = AddressablesAutoAddressConfig.RootFolder;
        
        if (!AssetDatabase.IsValidFolder(root))
        {
            report.AddWarning($"根目录不存在：{root}");
            report.Log("Addressables 根目录扫描失败");
            return report;
        }
        
        string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { root });
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            AddOrNormalizeAsset(settings, defaultGroup, assetPath, includeFolderEntry, dryRun, report);
        }
        
        if (!dryRun)
        {
            SaveSettings(settings);
        }
        
        report.Log(dryRun ? "Addressables 根目录扫描预览完成" : "Addressables 根目录扫描处理完成");
        return report;
    }

    
    /// <summary>
    /// 只处理当前 Addressables Groups 中已经存在的 Entry
    /// </summary>
    public static ProcessReport NormalizeExistingGroupEntries(bool includeFolderEntry, bool dryRun)
    {
        ProcessReport report = new ProcessReport();
        
        AddressableAssetSettings settings = GetSettings();
        if (settings == null)
            return report;

        foreach (AddressableAssetGroup group in settings.groups)
        {
            if (group == null)
                continue;
            
            List<AddressableAssetEntry> entries = group.entries.ToList();

            foreach (AddressableAssetEntry entry in entries)
            {
                if (entry == null)
                    continue;
                
                string assetPath = NormalizePath(entry.AssetPath);

                if (string.IsNullOrEmpty(assetPath))
                {
                    report.SkippedCount++;
                    continue;
                }
                
                if (!IsUnderRootFolder(assetPath))
                {
                    report.SkippedCount++;
                    continue;
                }
                
                bool isFolder = AssetDatabase.IsValidFolder(assetPath);
                
                if (isFolder && !includeFolderEntry)
                {
                    report.SkippedCount++;
                    continue;
                }
                
                AddOrNormalizeAsset(settings, group, assetPath, includeFolderEntry, dryRun, report);
            }
        }
        
        if (!dryRun)
        {
            SaveSettings(settings);
        }

        report.Log(dryRun ? "Addressables 已有 Entry 短地址修正预览完成" : "Addressables 已有 Entry 短地址修正完成");
        return report;
    }


    /// <summary>
    /// 把 Addressables Groups 中的文件夹 Entry 展开为具体文件 Entry
    /// </summary>
    public static ProcessReport ExpandFolderEntriesToAssets(bool dryRun)
    {
        ProcessReport report = new ProcessReport();
        
        AddressableAssetSettings settings = GetSettings();
        if (settings == null)
            return report;

        foreach (AddressableAssetGroup group in settings.groups)
        {
            if (group == null)
                continue;
            
            List<AddressableAssetEntry> entries = group.entries.ToList();

            foreach (AddressableAssetEntry entry in entries)
            {
                if (entry == null)
                    continue;

                string folderPath = NormalizePath(entry.AssetPath);
                
                if (!IsUnderRootFolder(folderPath))
                    continue;

                if (!AssetDatabase.IsValidFolder(folderPath))
                    continue;
                
                report.ScannedCount++;
                
                string[] childGuids = AssetDatabase.FindAssets(string.Empty, new[] { folderPath });

                foreach (string childGuid in childGuids)
                {
                    string childPath = NormalizePath(AssetDatabase.GUIDToAssetPath(childGuid));

                    if (childPath == folderPath)
                        continue;
                    
                    if (AssetDatabase.IsValidFolder(childPath))
                        continue;
                    
                    AddOrNormalizeAsset(settings, group, childPath, false, dryRun, report);
                }

                if (!dryRun)
                {
                    settings.RemoveAssetEntry(entry.guid, true);
                }
                
                report.RemovedCount++;
                report.AddMessage($"移除文件夹 Entry：{folderPath}");
            } 
        }
        
        if (!dryRun)
        {
            SaveSettings(settings);
        }

        report.Log(dryRun ? "文件夹 Entry 展开预览完成" : "文件夹 Entry 展开完成");
        return report;
    }


    /// <summary>
    /// 清理 Addressables Groups 中已经丢失或无效的 Entry
    /// </summary>
    public static ProcessReport CleanupMissingEntries(bool dryRun)
    {
        ProcessReport report = new ProcessReport();
        
        AddressableAssetSettings settings = GetSettings();
        if (settings == null)
            return report;

        foreach (AddressableAssetGroup group in settings.groups)
        {
            if (group == null)
                continue;
            
            List<AddressableAssetEntry> entries = group.entries.ToList();

            foreach (AddressableAssetEntry entry in entries)
            {
                if (entry == null)
                    continue;

                report.ScannedCount++;
                
                string assetPath = NormalizePath(entry.AssetPath);
                
                bool missing =
                    string.IsNullOrEmpty(assetPath) ||
                    (!AssetDatabase.IsValidFolder(assetPath) &&
                     AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) == null);
                
                if (!missing)
                    continue;
                
                if (!dryRun)
                {
                    settings.RemoveAssetEntry(entry.guid, true);
                }

                report.RemovedCount++;
                report.AddMessage($"移除丢失 Entry：GUID={entry.guid}，Address={entry.address}");
            }
        }
        
        if (!dryRun)
        {
            SaveSettings(settings);
        }

        report.Log(dryRun ? "丢失 Entry 清理预览完成" : "丢失 Entry 清理完成");
        return report;
    }


    /// <summary>
    /// 当资源从 Addressables 根目录移动到外部时，自动从 Addressables 中移除
    /// </summary>
    public static void RemoveEntryIfMovedOutOfRoot(string newAssetPath)
    {
        AddressableAssetSettings settings = GetSettings();
        if (settings == null)
            return;
        
        string guid = AssetDatabase.AssetPathToGUID(newAssetPath);
        
        if (string.IsNullOrEmpty(guid))
            return;
        
        AddressableAssetEntry entry = settings.FindAssetEntry(guid);
        
        if (entry == null)
            return;

        settings.RemoveAssetEntry(guid, true);
        SaveSettings(settings);
        
        Debug.Log($"资源已移出 Addressables 根目录，自动移除 Entry：{newAssetPath}");
    }


    /// <summary>
    /// 检查重复 Address
    /// </summary>
    public static ProcessReport CheckDuplicateAddresses()
    {
        ProcessReport report = new ProcessReport();
        
        AddressableAssetSettings settings = GetSettings();
        if (settings == null)
            return report;
        
        Dictionary<string, List<AddressableAssetEntry>> addressMap =
            new Dictionary<string, List<AddressableAssetEntry>>();

        foreach (AddressableAssetGroup group in settings.groups)
        {
            if (group == null)
                continue;


            foreach (AddressableAssetEntry entry in group.entries)
            {
                if (entry == null)
                    continue;
                
                report.ScannedCount++;

                if (string.IsNullOrEmpty(entry.address))
                    continue;
                
                if (!addressMap.TryGetValue(entry.address, out List<AddressableAssetEntry> list))
                {
                    list = new List<AddressableAssetEntry>();
                    addressMap.Add(entry.address, list);
                }

                list.Add(entry);
            }
        }

        foreach (KeyValuePair<string, List<AddressableAssetEntry>> pair in addressMap)
        {
            if (pair.Value.Count <= 1)
                continue;

            report.WarningCount++;
            
            string paths = string.Join(
                "\n",
                pair.Value.Select(e => $"    Group={e.parentGroup?.Name}, Path={e.AssetPath}"));
            
            report.AddMessage(
                $"[重复 Address] {pair.Key}\n{paths}");
        }
        
        report.Log("Addressables 重复 Address 检查完成");
        return report;
    }


    /// <summary>
    /// 保存 Addressables Settings
    /// </summary>
    private static void SaveSettings(AddressableAssetSettings settings)
    {
        if (settings != null)
        {
            EditorUtility.SetDirty(settings);
            
            foreach (AddressableAssetGroup group in settings.groups)
            {
                if (group != null)
                    EditorUtility.SetDirty(group);
            }
        }
        
        AssetDatabase.SaveAssets();
    }
    
    
    private static string NormalizePath(string path)
    {
        return AddressablesAutoAddressConfig.NormalizePath(path);
    }
}

/// <summary>
/// 当资源导入、移动、删除时，自动处理 Addressables 短地址
/// </summary>
public sealed class AddressablesAutoAddressPostprocessor : AssetPostprocessor
{
    private static bool _isProcessing;

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (_isProcessing)
            return;
        
        if (!AddressablesAutoAddressConfig.AutoProcessOnImport)
            return;
        
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        
        List<string> needProcessPaths = new List<string>();
        List<string> movedOutPaths = new List<string>();

        for (int i = 0; i < importedAssets.Length; i++)
        {
            string path = AddressablesAutoAddressConfig.NormalizePath(importedAssets[i]);
            
            if (AddressablesAutoAddressUtility.IsUnderRootFolder(path))
                needProcessPaths.Add(path);
        }

        for (int i = 0; i < movedAssets.Length; i++)
        {
            string newPath = AddressablesAutoAddressConfig.NormalizePath(movedAssets[i]);
            string oldPath = AddressablesAutoAddressConfig.NormalizePath(movedFromAssetPaths[i]);
            
            bool oldInRoot = AddressablesAutoAddressUtility.IsUnderRootFolder(oldPath);
            bool newInRoot = AddressablesAutoAddressUtility.IsUnderRootFolder(newPath);
            
            if (newInRoot)
                needProcessPaths.Add(newPath);
            else if (oldInRoot)
                movedOutPaths.Add(newPath);
        }
        
        bool hasDeletedUnderRoot = false;

        for (int i = 0; i < deletedAssets.Length; i++)
        {
            string path = AddressablesAutoAddressConfig.NormalizePath(deletedAssets[i]);
            
            if (AddressablesAutoAddressUtility.IsUnderRootFolder(path))
            {
                hasDeletedUnderRoot = true;
                break;
            }
        }
        
        if (needProcessPaths.Count == 0 && movedOutPaths.Count == 0 && !hasDeletedUnderRoot)
            return;

        EditorApplication.delayCall += () =>
        {
            if (_isProcessing)
                return;

            try
            {
                _isProcessing = true;
                
                AddressableAssetSettings settings = AddressablesAutoAddressUtility.GetSettings();
                if (settings == null)
                    return;
                
                AddressableAssetGroup defaultGroup = AddressablesAutoAddressUtility.GetDefaultGroup(settings);

                AddressablesAutoAddressUtility.ProcessReport report =
                    new AddressablesAutoAddressUtility.ProcessReport();
                
                bool includeFolderEntry = AddressablesAutoAddressConfig.IncludeFolderEntries;

                for (int i = 0; i < needProcessPaths.Count; i++)
                {
                    AddressablesAutoAddressUtility.AddOrNormalizeAsset(
                        settings,
                        defaultGroup,
                        needProcessPaths[i],
                        includeFolderEntry,
                        false,
                        report);
                }
                
                for (int i = 0; i < movedOutPaths.Count; i++)
                {
                    AddressablesAutoAddressUtility.RemoveEntryIfMovedOutOfRoot(movedOutPaths[i]);
                }
                
                if (hasDeletedUnderRoot)
                {
                    AddressablesAutoAddressUtility.CleanupMissingEntries(false);
                }
                
                if (report.AddedCount > 0 || report.RenamedCount > 0 || report.WarningCount > 0)
                {
                    report.Log("Addressables 自动导入处理完成");
                }
                
                AssetDatabase.SaveAssets();
            }
            finally
            {
                _isProcessing = false;
            }
        };
    }
}


/// <summary>
/// Addressables 自动短地址工具窗口
/// </summary>
public sealed class AddressablesAutoAddressToolWindow : EditorWindow
{
    private bool _dryRun;
    private Vector2 _scroll;

    
    [MenuItem("Tools/Addressables/自动短地址工具")]
    public static void Open()
    {
        AddressablesAutoAddressToolWindow window =
            GetWindow<AddressablesAutoAddressToolWindow>("Addressables 短地址工具");

        window.minSize = new Vector2(640, 520);
        window.Show();
    }
    
    
    [MenuItem("Tools/Addressables/一键扫描根目录并生成短地址")]
    public static void MenuScanRoot()
    {
        AddressablesAutoAddressUtility.ScanRootFolderAndAddMissingAssets(
            AddressablesAutoAddressConfig.IncludeFolderEntries,
            false);
    }
    
    
    [MenuItem("Tools/Addressables/修正Groups已有Entry短地址")]
    public static void MenuNormalizeExisting()
    {
        AddressablesAutoAddressUtility.NormalizeExistingGroupEntries(
            AddressablesAutoAddressConfig.IncludeFolderEntries,
            false);
    }
    
    
    [MenuItem("Tools/Addressables/展开文件夹Entry为具体资源")]
    public static void MenuExpandFolders()
    {
        AddressablesAutoAddressUtility.ExpandFolderEntriesToAssets(false);
    }
    
    
    [MenuItem("Tools/Addressables/清理丢失Entry")]
    public static void MenuCleanupMissing()
    {
        AddressablesAutoAddressUtility.CleanupMissingEntries(false);
    }
    
    
    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        DrawHeader();
        DrawSettings();
        DrawActions();
        DrawTips();

        EditorGUILayout.EndScrollView();
    }


    private void DrawHeader()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Addressables 自动短地址工具", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);
        
        EditorGUILayout.HelpBox(
            "该工具用于把 Assets/GameRes/AddressableAssets 下的资源自动加入 Addressables，" +
            "并把完整路径 Address 转换为短地址，例如 Prefab/DefaultPrefab。",
            MessageType.Info);
    }


    private void DrawSettings()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("基础设置", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        
        string rootFolder = EditorGUILayout.TextField(
            "Addressables资源根目录",
            AddressablesAutoAddressConfig.RootFolder);

        if (GUILayout.Button("选择根目录", GUILayout.Height(24)))
        {
            string absolutePath = EditorUtility.OpenFolderPanel(
                "选择 Addressables 资源根目录",
                Application.dataPath,
                string.Empty);

            if (!string.IsNullOrEmpty(absolutePath))
            {
                string projectPath = Application.dataPath.Replace("\\", "/").Replace("/Assets", string.Empty);
                absolutePath = absolutePath.Replace("\\", "/");
                
                if (absolutePath.StartsWith(projectPath, StringComparison.Ordinal))
                {
                    rootFolder = absolutePath.Substring(projectPath.Length + 1);
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "路径无效",
                        "请选择当前 Unity 工程 Assets 目录下的文件夹。",
                        "确定");
                }
            }
        }
        
        bool autoProcess = EditorGUILayout.Toggle(
            "导入时自动处理",
            AddressablesAutoAddressConfig.AutoProcessOnImport);
        
        bool includeFolderEntries = EditorGUILayout.Toggle(
            "文件夹也作为Entry",
            AddressablesAutoAddressConfig.IncludeFolderEntries);
        
        _dryRun = EditorGUILayout.Toggle(
            "预览模式，不实际修改",
            _dryRun);
        
        if (EditorGUI.EndChangeCheck())
        {
            AddressablesAutoAddressConfig.RootFolder = rootFolder;
            AddressablesAutoAddressConfig.AutoProcessOnImport = autoProcess;
            AddressablesAutoAddressConfig.IncludeFolderEntries = includeFolderEntries;
        }
        
        EditorGUILayout.HelpBox(
            "建议关闭“文件夹也作为Entry”。更推荐把文件夹内的具体 Prefab、Sprite、Audio、Scene、Config 等资源逐个加入 Addressables。",
            MessageType.Warning);
    }
    
    
    [MenuItem("Tools/Addressables/检查重复Address")]
    public static void MenuCheckDuplicate()
    {
        AddressablesAutoAddressUtility.CheckDuplicateAddresses();
    }


    private void DrawActions()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("批量处理", EditorStyles.boldLabel);
        
        
        if (GUILayout.Button("1. 扫描根目录：添加缺失资源并生成短地址", GUILayout.Height(34)))
        {
            AddressablesAutoAddressUtility.ScanRootFolderAndAddMissingAssets(
                AddressablesAutoAddressConfig.IncludeFolderEntries,
                _dryRun);
        }
        
        
        if (GUILayout.Button("2. 修正 Addressables Groups 中已有 Entry 的短地址", GUILayout.Height(34)))
        {
            AddressablesAutoAddressUtility.NormalizeExistingGroupEntries(
                AddressablesAutoAddressConfig.IncludeFolderEntries,
                _dryRun);
        }
        
        
        if (GUILayout.Button("3. 展开文件夹 Entry 为具体资源 Entry", GUILayout.Height(34)))
        {
            bool confirm = EditorUtility.DisplayDialog(
                "确认展开文件夹 Entry",
                "该操作会把 Addressables Groups 中的文件夹 Entry 展开为具体资源 Entry，并移除文件夹 Entry。\n\n推荐执行此操作，因为业务层通常加载具体资源，而不是文件夹。",
                "确认执行",
                "取消");

            if (confirm)
            {
                AddressablesAutoAddressUtility.ExpandFolderEntriesToAssets(_dryRun);
            }
        }
        
        
        if (GUILayout.Button("4. 清理丢失或无效 Entry", GUILayout.Height(34)))
        {
            AddressablesAutoAddressUtility.CleanupMissingEntries(_dryRun);
        }
        
        
        if (GUILayout.Button("5. 检查重复 Address", GUILayout.Height(34)))
        {
            AddressablesAutoAddressUtility.CheckDuplicateAddresses();
        }
        
        
        EditorGUILayout.Space(8);


        if (GUILayout.Button("推荐一键处理：展开文件夹 → 扫描根目录 → 修正已有Entry → 检查重复", GUILayout.Height(38)))
        {
            bool confirm = EditorUtility.DisplayDialog(
                "确认执行推荐一键处理",
                "该操作会：\n" +
                "1. 展开文件夹 Entry 为具体资源 Entry\n" +
                "2. 扫描根目录并添加缺失资源\n" +
                "3. 修正已有 Entry 短地址\n" +
                "4. 检查重复 Address\n\n" +
                "如果你不确定，可以先勾选“预览模式”。",
                "确认执行",
                "取消");

            if (confirm)
            {
                AddressablesAutoAddressUtility.ExpandFolderEntriesToAssets(_dryRun);

                AddressablesAutoAddressUtility.ScanRootFolderAndAddMissingAssets(
                    AddressablesAutoAddressConfig.IncludeFolderEntries,
                    _dryRun);

                AddressablesAutoAddressUtility.NormalizeExistingGroupEntries(
                    AddressablesAutoAddressConfig.IncludeFolderEntries,
                    _dryRun);

                AddressablesAutoAddressUtility.CheckDuplicateAddresses();
            }
        }
    }


    private void DrawTips()
    {
        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("使用说明", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "生成规则：\n" +
            "1. 去掉根目录：Assets/GameRes/AddressableAssets/\n" +
            "2. 去掉文件后缀：.prefab / .png / .unity / .asset / .mp3 等\n" +
            "3. 保留资源分类目录\n\n" +
            "示例：\n" +
            "Assets/GameRes/AddressableAssets/Prefab/DefaultPrefab.prefab\n" +
            "→ Prefab/DefaultPrefab",
            MessageType.None);
        
        EditorGUILayout.HelpBox(
            "注意：修改 Addressables Entry 后，如果 Play Mode Script 使用 Use Existing Build，必须重新 Build Addressables，否则运行时 Catalog 仍然是旧地址。",
            MessageType.Warning);
        
        EditorGUILayout.Space(8);
        
        if (GUILayout.Button("打开 Addressables Groups", GUILayout.Height(28)))
        {
            EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
        }
    }
}

#endif
