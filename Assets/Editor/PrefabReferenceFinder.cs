using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrefabReferenceFinder : EditorWindow
{
    private GameObject _rootObject;
    private Object targetAsset;

    private List<ReferenceResult> _results = new();
    private Vector2 _scrollPos;

    private struct ReferenceResult
    {
        public GameObject GameObject;
        public Component Component;
        public string PropertyName;
        public string Path;
    }

    [MenuItem("Tools/预制体引用查找")]
    public static void ShowWindow()
    {
        GetWindow<PrefabReferenceFinder>("引用查找器");
    }

    private void OnGUI()
    {
        GUILayout.Label("查找配置", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        _rootObject = (GameObject)EditorGUILayout.ObjectField("根节点 (预制体)", _rootObject, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck() && _rootObject == null)
        {
            if (Selection.activeGameObject != null)
                _rootObject = Selection.activeGameObject;
        }

        targetAsset = EditorGUILayout.ObjectField("要查找的资源", targetAsset, typeof(Object), true);

        EditorGUILayout.Space();

        GUI.enabled = _rootObject != null && targetAsset != null;
        if (GUILayout.Button("开始查找引用", GUILayout.Height(30)))
        {
            FindReferences();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();
        GUILayout.Label($"查找结果 (找到: {_results.Count} 个)", EditorStyles.boldLabel);

        DrawResultList();
    }

    private void DrawResultList()
    {
        if (_results.Count == 0) return;

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        foreach (var res in _results)
        {
            EditorGUILayout.BeginVertical("box");
            
            // 显示路径
            GUILayout.Label($"路径: {res.Path}", EditorStyles.wordWrappedLabel);
            
            EditorGUILayout.BeginHorizontal();
            // 显示组件和属性名
            GUILayout.Label($"组件: {res.Component.GetType().Name} -> 属性: {res.PropertyName}", EditorStyles.miniLabel);
            
            // 跳转按钮
            if (GUILayout.Button("选中物体", GUILayout.Width(80)))
            {
                Selection.activeGameObject = res.GameObject;
                EditorGUIUtility.PingObject(res.GameObject);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    private void FindReferences()
    {
   _results.Clear();

        if (_rootObject == null || targetAsset == null) return;

        Texture2D targetTexture = targetAsset as Texture2D;
        bool isTargetTexture = targetTexture != null;

        // 获取根节点下所有组件（包括隐藏的子物体，也包括根节点自身）
        Component[] allComponents = _rootObject.GetComponentsInChildren<Component>(true);

        float progress = 0f;
        int total = allComponents.Length;

        for (int i = 0; i < total; i++)
        {
            Component comp = allComponents[i];
            
            if (i % 20 == 0)
            {
                EditorUtility.DisplayProgressBar("查找中...", $"正在扫描: {comp?.gameObject.name}", (float)i / total);
            }

            if (comp == null) continue;

            SerializedObject so = new SerializedObject(comp);
            SerializedProperty sp = so.GetIterator();

            // 遍历属性
            while (sp.NextVisible(true)) 
            {
                if (sp.propertyType == SerializedPropertyType.ObjectReference)
                {
                    bool isMatch = false;
                    Object propValue = sp.objectReferenceValue;
                    
                    if (propValue == targetAsset)
                    {
                        isMatch = true;
                    }
                    
                    else if (isTargetTexture && propValue is Sprite sprite)
                    {
                        // 检查这个 Sprite 是否属于我们查找的 Texture
                        if (sprite.texture == targetTexture)
                        {
                            isMatch = true;
                        }
                    }

                    if (isMatch)
                    {
                        _results.Add(new ReferenceResult
                        {
                            GameObject = comp.gameObject,
                            Component = comp,
                            PropertyName = sp.displayName,
                            Path = GetHierarchyPath(comp.transform)
                        });
                    }
                }
            }
        }

        EditorUtility.ClearProgressBar();
        
        if (_results.Count == 0)
        {
            EditorUtility.DisplayDialog("查找结束", "未在预制体中找到该资源的引用。\n请确认资源是否正确，或是否在非Public属性中。", "确定");
        }
    }

    // 辅助方法：生成从根节点到子物体的完整路径
    private string GetHierarchyPath(Transform target)
    {
        if (target == _rootObject.transform) return target.name;

        string path = target.name;
        Transform current = target.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            
            // 如果追溯到了我们要查找的根节点，就停止，这样路径更清晰
            if (current == _rootObject.transform) break;
            
            current = current.parent;
        }

        return path;
    }
}
