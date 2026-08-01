using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class ProjectReferenceFinder : EditorWindow
{
    // 查找目标信息
    private string[] _targetGuids;
    private string[] _targetPaths;
    
    // 结果列表
    private List<string> _resultPaths = new List<string>();
    private Vector2 _scrollPos;

    private string _currentSelectedPath = "";
    
    // 可以在这里扩展想要查找的文件类型
    private static readonly string[] SearchExtensions = new string[] 
    { 
        ".prefab", ".unity", ".mat", ".asset", ".controller" 
    };

    [MenuItem("Tools/查找资源引用 (窗口化)", false)]
    [MenuItem("Assets/查找资源引用 (窗口化)", false, 2)]
    static void Init()
    {
        if (Selection.assetGUIDs.Length == 0)
        {
            Debug.LogError("请先在Project窗口选中一个或多个资源。");
            return;
        }

        // 创建窗口
        ProjectReferenceFinder window = GetWindow<ProjectReferenceFinder>("工程引用查找");
        window.SetupTargets(Selection.assetGUIDs);
        window.Show();
    }
    
    public void SetupTargets(string[] guids)
    {
        _targetGuids = guids;
        _targetPaths = new string[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            _targetPaths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
        }
        _resultPaths.Clear();
    }

    private void OnGUI()
    {
        GUILayout.Label("查找目标资源引用", EditorStyles.boldLabel);
        
        if (_targetPaths != null && _targetPaths.Length > 0)
        {
            string paths = string.Join("\n", _targetPaths);
            EditorGUILayout.HelpBox($"当前选中: {_targetPaths.Length} 个文件\n{paths}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("未选中目标资源", MessageType.Warning);
        }

        if (GUILayout.Button("开始全工程扫描", GUILayout.Height(30)))
        {
            FindReferences();
        }

        EditorGUILayout.Space();
        
        GUILayout.Label($"查找结果: {_resultPaths.Count} 个引用文件", EditorStyles.boldLabel);
        
        if (_resultPaths.Count > 0)
        {
            DrawResultList();
        }
    }

    private void DrawResultList()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        foreach (string path in _resultPaths)
        {
            bool isSelected = (path == _currentSelectedPath);
            if (isSelected)
            {
                GUI.backgroundColor = Color.cyan; 
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.BeginHorizontal("box");

            Texture icon = AssetDatabase.GetCachedIcon(path);
            GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));

            if (isSelected)
                GUILayout.Label(path, EditorStyles.boldLabel);
            else
                GUILayout.Label(path, EditorStyles.wordWrappedLabel);

            GUILayout.FlexibleSpace();
            
            string btnText = isSelected ? "已选" : "选中";
            
            if (GUILayout.Button(btnText, GUILayout.Width(60)))
            {
                _currentSelectedPath = path;
                SelectObject(path);
            }

            EditorGUILayout.EndHorizontal();
            
            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.EndScrollView();
    }

    private void FindReferences()
    {
        _resultPaths.Clear();

        if (_targetGuids == null || _targetGuids.Length == 0) return;

        // 获取工程中所有文件路径
        string[] allFiles = AssetDatabase.GetAllAssetPaths();
        
        // 过滤文件类型（只查Prefab, Scene, Material等文本格式的资源）
        var filesToSearch = allFiles.Where(path => 
            SearchExtensions.Any(ext => path.EndsWith(ext))
        ).ToArray();

        int total = filesToSearch.Length;
        int matchCount = 0;

        // 开始遍历
        for (int i = 0; i < total; i++)
        {
            string filePath = filesToSearch[i];

            // 更新进度条
            if (i % 20 == 0)
            {
                EditorUtility.DisplayProgressBar("正在扫描整个工程...",
                    $"正在分析 ({i}/{total}): {Path.GetFileName(filePath)}",
                    (float)i / total);
            }

            // 读取文件文本内容
            string content = File.ReadAllText(filePath);
            
            // 检查内容中是否包含我们的 GUID
            bool found = false;
            foreach (var targetGuid in _targetGuids)
            {
                if (content.IndexOf(targetGuid) != -1) // -1 表示没找到
                {
                    found = true;
                    break;
                }
            }

            if (found)
            {
                _resultPaths.Add(filePath);
                matchCount++;
            }
        }

        EditorUtility.ClearProgressBar();
        Debug.Log($"查找完成，共发现 {matchCount} 处引用。");
    }

    private void SelectObject(string path)
    {
        Object obj = AssetDatabase.LoadMainAssetAtPath(path);
        if (obj != null)
        {
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }
        else
        {
            Debug.LogWarning("无法加载该资源，可能文件已丢失: " + path);
        }
    }
}