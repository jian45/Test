using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;


class TestCreate
{
    [MenuItem("GameObject/秘制工具/生成UI面板代码", false, -2)]
    public static void CreateUICode()
    {
        GameObject go = Selection.activeObject as GameObject;
        UIPanelCodeSpawner.SpawnUIPanelCode(go);
    }
    [MenuItem("GameObject/秘制工具/生成Item面板代码", false, -2)]
    public static void CreateItemCode()
    {
        GameObject go = Selection.activeObject as GameObject;
        UIItemCodeSpawner.SpawnUIPanelCode(go);
    }
}


/// <summary>
/// 生成UI面板代码
/// </summary>
public class UIPanelCodeSpawner
{

    private static Dictionary<string, List<Component>> Path2WidgetCachedDict = null;
    private static List<string> WidgetInterfaceList = null;

    private const string UIPanelPrefix = "UI";
    private const string UIWidgetPrefix = "el_";
    //跳过不读item中的
    private const string JumpName = "Item_";

    public static void SpawnUIPanelCode(GameObject gameObject)
    {
        if (null == gameObject)
        {
            Debug.LogError("对象是空,不能生成代码");
            return;
        }

        try
        {
            string uiName = gameObject.name;
            if (uiName.StartsWith(UIPanelPrefix))
            {
                Debug.LogWarning($"----------开始生成UI {uiName} 相关代码 ----------");
                SpawnUICode(gameObject);
                Debug.LogWarning($"生成UI  {uiName} 完毕!!!");
                return;
            }
            Debug.LogError($"选择的预设物不属于 UI , 请检查 {uiName}！！！！！！");
        }
        finally
        {
            Path2WidgetCachedDict?.Clear();
            Path2WidgetCachedDict = null;
        }
    }


    private static void SpawnUICode(GameObject gameObject)
    {
        Path2WidgetCachedDict?.Clear();
        Path2WidgetCachedDict = new Dictionary<string, List<Component>>();

        FindAllWidgets(gameObject.transform, "");

        SpawnBindCodeForUI(gameObject);
        SpawnCodeForUI(gameObject);


        AssetDatabase.Refresh();
        Component component = gameObject.GetComponent(gameObject.name);
        if (component == null)
        {

            Type componentType = Type.GetType(gameObject.name + ", Assembly-CSharp");
            if (componentType != null)
            {
                gameObject.AddComponent(componentType);

            }
        }

    }

    /// <summary>
    /// 绑定的ui
    /// </summary>
    /// <param name="gameObject"></param>
    private static void SpawnBindCodeForUI(GameObject gameObject)
    {

        if (null == gameObject)
        {
            return;
        }
        //ui代码名
        string strUIName = gameObject.name;

        //文件夹路径
        string strFilePath = Application.dataPath + "/Scripts/UI/UI/GenerateUI";
        if (!Directory.Exists(strFilePath))
        {
            Directory.CreateDirectory(strFilePath);
        }
        strFilePath = Application.dataPath + "/Scripts/UI/UI/GenerateUI/" + strUIName + "Model.cs";
        StreamWriter sw = new StreamWriter(strFilePath, false, Encoding.UTF8);

        StringBuilder strBuilder = new StringBuilder();
        strBuilder.AppendLine()
            .AppendLine("using UnityEngine;");
        strBuilder.AppendLine("using UnityEngine.UI;");
        strBuilder.AppendLine("using ClientFramework.UI;");


        #region 核心

        strBuilder.AppendFormat("public partial class {0} \r\n", strUIName)
            .AppendLine("{");

        CreateWidgetBindCode(ref strBuilder, gameObject.transform);

        strBuilder.AppendLine("    private bool isInit = false;");

        CreateWidgetBindCodeMethod(ref strBuilder, gameObject.transform);

        CreateDestroyWidgetCode(ref strBuilder);

        strBuilder.AppendLine("}");

        #endregion

        sw.Write(strBuilder);
        sw.Flush();
        sw.Close();
    }

    /// <summary>
    /// 挂载的ui
    /// </summary>
    /// <param name="gameObject"></param>
    private static void SpawnCodeForUI(GameObject gameObject)
    {

        if (null == gameObject)
        {
            return;
        }
        //ui代码名
        string strUIName = gameObject.name;

        //文件夹路径
        string strFilePath = Application.dataPath + "/Scripts/UI/UI";
        if (!Directory.Exists(strFilePath))
        {
            Directory.CreateDirectory(strFilePath);
        }


        strFilePath = Application.dataPath + "/Scripts/UI/UI/" + strUIName + ".cs";

        if (File.Exists(strFilePath))
        {
            Debug.LogError("绑定代码已存在");
            return;
        }


        StreamWriter sw = new StreamWriter(strFilePath, false, Encoding.UTF8);

        StringBuilder strBuilder = new StringBuilder();
        strBuilder.AppendLine("using UnityEngine;");
        strBuilder.AppendLine("using UnityEngine.UI;");
        strBuilder.AppendLine("using ClientFramework.UI;");
        strBuilder.AppendLine();

        #region 核心

        strBuilder.AppendFormat("public partial class {0} :UIBase\r\n", strUIName)
           .AppendLine("{");

        strBuilder.AppendLine("    void Start()");
        strBuilder.AppendLine("    {");
        strBuilder.AppendLine("        ");
        strBuilder.AppendLine("    }");

        strBuilder.AppendLine("    void Update()");
        strBuilder.AppendLine("    {");
        strBuilder.AppendLine("        ");
        strBuilder.AppendLine("    }");

        strBuilder.AppendLine("}");

        #endregion

        sw.Write(strBuilder);
        sw.Flush();
        sw.Close();
    }

    /// <summary>
    /// 销毁的代码
    /// </summary>
    /// <param name="strBuilder"></param>
    /// <param name="isScrollItem"></param>
    public static void CreateDestroyWidgetCode(ref StringBuilder strBuilder, bool isScrollItem = false)
    {

    }


    /// <summary>
    /// 创建ui代码
    /// </summary>
    /// <param name="strBuilder"></param>
    /// <param name="transRoot"></param>
    private static void CreateWidgetBindCode(ref StringBuilder strBuilder, Transform transRoot)
    {
        foreach (KeyValuePair<string, List<Component>> pair in Path2WidgetCachedDict)
        {
            foreach (var info in pair.Value)
            {
                Component widget = info;
                //目标路径
                string path = GetWidgetPath(widget.transform, transRoot);

                if (path.Contains(UIPanelPrefix) || path.Contains(JumpName))
                {
                    continue;
                }

                string strClassType = widget.GetType().ToString();
                string strInterfaceType = strClassType;


                string widgetName = (widget.name + strClassType.Split('.').ToList().Last()).Replace(strClassType, "");

                if (string.IsNullOrEmpty(widgetName))
                {
                    //等于空说明是item类
                    widgetName = strClassType;
                }
                else
                {
                    widgetName = widget.name + strClassType.Split('.').ToList().Last();
                }

                strBuilder.AppendFormat("    private {0} {1};\r\n", strInterfaceType, widgetName);
            }
        }
    }
    private static string GetWidgetPath(Transform obj, Transform root)
    {
        string path = obj.name;

        while (obj.parent != null && obj.parent != root)
        {
            obj = obj.transform.parent;
            path = obj.name + "/" + path;
        }
        return path;
    }
    /// <summary>
    /// 绑定代码的方法
    /// </summary>
    /// <param name="strBuilder"></param>
    /// <param name="transRoot"></param>
    private static void CreateWidgetBindCodeMethod(ref StringBuilder strBuilder, Transform transRoot)
    {
        strBuilder.AppendLine("");
        strBuilder.AppendLine("    public virtual void InitUI(GameObject uipanel)");
        strBuilder.AppendLine("    {");
        //如果初始化过就返回
        strBuilder.AppendLine("        if (isInit)");
        strBuilder.AppendLine("        {");
        strBuilder.AppendLine("            return;");
        strBuilder.AppendLine("        }");
        strBuilder.AppendLine("        isInit = true;");

        foreach (KeyValuePair<string, List<Component>> pair in Path2WidgetCachedDict)
        {
            foreach (var info in pair.Value)
            {
                Component widget = info;
                //目标路径
                string path = GetWidgetPath(widget.transform, transRoot);

                if (path.Contains(UIPanelPrefix) || path.Contains(JumpName))
                {
                    continue;
                }

                //组件type 名
                string strClassType = widget.GetType().ToString();
                //组件名
                string componentName = strClassType.Split('.').ToList().Last();
                //变量名

                string codeName = (widget.name + strClassType.Split('.').ToList().Last()).Replace(strClassType, "");

                if (string.IsNullOrEmpty(codeName))
                {
                    //等于空说明是item类
                    codeName = strClassType;
                }
                else
                {
                    codeName = widget.name + strClassType.Split('.').ToList().Last();
                }

                strBuilder.AppendFormat("\t\t{0} = transform.Find(\"{1}\").GetComponent<{2}>();\r\n", codeName, path, componentName);

            }
        }
        strBuilder.AppendLine("        ");

        strBuilder.AppendLine("    }");
    }

    private static void FindAllWidgets(Transform trans, string strPath)
    {
        if (null == trans)
        {
            return;
        }
        for (int nIndex = 0; nIndex < trans.childCount; ++nIndex)
        {


            Transform child = trans.GetChild(nIndex);
            string strTemp = strPath + "/" + child.name;

            //不获取Item下的el 代码, 但是获取item自身的
            if (child.gameObject.name.StartsWith(JumpName))
            {
                Component component = child.GetComponent(child.name);
                //只获取itemzi自身
                if (null == component || !component.GetType().Name.StartsWith(JumpName))
                {
                    continue;
                }

                if (Path2WidgetCachedDict.ContainsKey(child.name))
                {
                    Path2WidgetCachedDict[child.name].Add(component);
                    continue;
                }

                List<Component> componentsList = new List<Component>();
                componentsList.Add(component);
                Path2WidgetCachedDict.Add(child.name, componentsList);

            }
            else
            {
                if (child.name.StartsWith(UIWidgetPrefix))
                {
                    foreach (var uiComponent in WidgetInterfaceList)
                    {
                        Component component = child.GetComponent(uiComponent);
                        if (null == component)
                        {
                            continue;
                        }

                        if (Path2WidgetCachedDict.ContainsKey(child.name))
                        {
                            Path2WidgetCachedDict[child.name].Add(component);
                            continue;
                        }

                        List<Component> componentsList = new List<Component>();
                        componentsList.Add(component);
                        Path2WidgetCachedDict.Add(child.name, componentsList);
                    }
                }

                FindAllWidgets(child, strTemp);
            }




        }
    }


    static UIPanelCodeSpawner()
    {
        WidgetInterfaceList = new List<string>();
        WidgetInterfaceList.Add("ButtonExt");
        WidgetInterfaceList.Add("Text");
        WidgetInterfaceList.Add("TMPro.TextMeshProUGUI");
        WidgetInterfaceList.Add("InputField");
        WidgetInterfaceList.Add("Scrollbar");
        WidgetInterfaceList.Add("ToggleGroup");
        WidgetInterfaceList.Add("Toggle");
        WidgetInterfaceList.Add("Dropdown");
        WidgetInterfaceList.Add("Slider");
        WidgetInterfaceList.Add("ScrollRect");
        WidgetInterfaceList.Add("Image");
        WidgetInterfaceList.Add("RawImage");
        WidgetInterfaceList.Add("RectTransform");
        WidgetInterfaceList.Add("SlidingBlock");
        WidgetInterfaceList.Add("CanvasGroup");

    }


}

/// <summary>
/// 生成item代码
/// </summary>
public class UIItemCodeSpawner
{

    private static Dictionary<string, List<Component>> Path2WidgetCachedDict = null;
    private static List<string> WidgetInterfaceList = null;

    private const string UIPanelPrefix = "Item_";
    private const string UIWidgetPrefix = "el_";
    //跳过不读item中的
    private const string JumpName = "UI";

    public static void SpawnUIPanelCode(GameObject gameObject)
    {
        if (null == gameObject)
        {
            Debug.LogError("对象是空,不能生成代码");
            return;
        }

        try
        {
            string uiName = gameObject.name;
            if (uiName.StartsWith(UIPanelPrefix))
            {
                Debug.LogWarning($"----------开始生成Item {uiName} 相关代码 ----------");
                SpawnUICode(gameObject);
                Debug.LogWarning($"生成UI  {uiName} 完毕!!!");
                return;
            }
            Debug.LogError($"选择的预设物不属于 Item , 请检查 {uiName}！！！！！！");
        }
        finally
        {
            Path2WidgetCachedDict?.Clear();
            Path2WidgetCachedDict = null;
        }
    }


    private static void SpawnUICode(GameObject gameObject)
    {
        Path2WidgetCachedDict?.Clear();
        Path2WidgetCachedDict = new Dictionary<string, List<Component>>();

        FindAllWidgets(gameObject.transform, "");

        SpawnBindCodeForUI(gameObject);
        SpawnCodeForUI(gameObject);


        AssetDatabase.Refresh();

        Component component = gameObject.GetComponent(gameObject.name);
        if (component == null)
        {

            Type componentType = Type.GetType(gameObject.name + ", Assembly-CSharp");
            if (componentType != null)
            {
                gameObject.AddComponent(componentType);

            }
        }

    }

    /// <summary>
    /// 绑定的ui
    /// </summary>
    /// <param name="gameObject"></param>
    private static void SpawnBindCodeForUI(GameObject gameObject)
    {

        if (null == gameObject)
        {
            return;
        }
        //ui代码名
        string strUIName = gameObject.name;

        //文件夹路径
        string strFilePath = Application.dataPath + "/Scripts/UI/Item/GenerateUI";
        if (!Directory.Exists(strFilePath))
        {
            Directory.CreateDirectory(strFilePath);
        }
        strFilePath = Application.dataPath + "/Scripts/UI/Item/GenerateUI/" + strUIName + "Model.cs";
        StreamWriter sw = new StreamWriter(strFilePath, false, Encoding.UTF8);

        StringBuilder strBuilder = new StringBuilder();
        strBuilder.AppendLine("using UnityEngine;");
        strBuilder.AppendLine("using UnityEngine.UI;");
        strBuilder.AppendLine("using ClientFramework.UI;");

        #region 核心

        strBuilder.AppendFormat("public partial class {0} \r\n", strUIName)
            .AppendLine("{");
        strBuilder.AppendLine("    private bool isInit = false;");

        CreateWidgetBindCode(ref strBuilder, gameObject.transform);
        CreateWidgetBindCodeMethod(ref strBuilder, gameObject.transform);

        CreateDestroyWidgetCode(ref strBuilder);

        strBuilder.AppendLine("}");

        #endregion

        sw.Write(strBuilder);
        sw.Flush();
        sw.Close();
    }

    /// <summary>
    /// 挂载的ui
    /// </summary>
    /// <param name="gameObject"></param>
    private static void SpawnCodeForUI(GameObject gameObject)
    {

        if (null == gameObject)
        {
            return;
        }
        //ui代码名
        string strUIName = gameObject.name;

        //文件夹路径
        string strFilePath = Application.dataPath + "/Scripts/UI/Item";
        if (!Directory.Exists(strFilePath))
        {
            Directory.CreateDirectory(strFilePath);
        }


        strFilePath = Application.dataPath + "/Scripts/UI/Item/" + strUIName + ".cs";

        if (File.Exists(strFilePath))
        {
            Debug.LogError("绑定代码已存在");
            return;
        }


        StreamWriter sw = new StreamWriter(strFilePath, false, Encoding.UTF8);

        StringBuilder strBuilder = new StringBuilder();
        strBuilder.AppendLine("using UnityEngine;");
        strBuilder.AppendLine("using UnityEngine.UI;");
        strBuilder.AppendLine("using ClientFramework.UI;");

        #region 核心

        strBuilder.AppendFormat("public partial class {0} :MonoBehaviour\r\n", strUIName)
           .AppendLine("{");

        strBuilder.AppendLine("    void Start()");
        strBuilder.AppendLine("    {");
        strBuilder.AppendLine("        ");
        strBuilder.AppendLine("    }");

        strBuilder.AppendLine("    void Update()");
        strBuilder.AppendLine("    {");
        strBuilder.AppendLine("        ");
        strBuilder.AppendLine("    }");

        strBuilder.AppendLine("}");

        #endregion

        sw.Write(strBuilder);
        sw.Flush();
        sw.Close();
    }

    /// <summary>
    /// 销毁的代码
    /// </summary>
    /// <param name="strBuilder"></param>
    /// <param name="isScrollItem"></param>
    public static void CreateDestroyWidgetCode(ref StringBuilder strBuilder, bool isScrollItem = false)
    {

    }


    /// <summary>
    /// 创建ui代码
    /// </summary>
    /// <param name="strBuilder"></param>
    /// <param name="transRoot"></param>
    private static void CreateWidgetBindCode(ref StringBuilder strBuilder, Transform transRoot)
    {
        foreach (KeyValuePair<string, List<Component>> pair in Path2WidgetCachedDict)
        {
            foreach (var info in pair.Value)
            {


                Component widget = info;

                //目标路径
                string path = GetWidgetPath(widget.transform, transRoot);

                if (path.Contains(UIPanelPrefix) || path.Contains(JumpName))
                {
                    continue;
                }


                string strClassType = widget.GetType().ToString();
                string strInterfaceType = strClassType;
                //组件名
                string componentName = strClassType.Split('.').ToList().Last();

                string widgetName = widget.name + componentName;

                strBuilder.AppendFormat("    private {0} {1};\r\n", strInterfaceType, widgetName);
            }
        }
    }
    private static string GetWidgetPath(Transform obj, Transform root)
    {
        string path = obj.name;

        while (obj.parent != null && obj.parent != root)
        {
            obj = obj.transform.parent;
            path = obj.name + "/" + path;
        }
        return path;
    }
    /// <summary>
    /// 绑定代码的方法
    /// </summary>
    /// <param name="strBuilder"></param>
    /// <param name="transRoot"></param>
    private static void CreateWidgetBindCodeMethod(ref StringBuilder strBuilder, Transform transRoot)
    {
        strBuilder.AppendLine("");
        strBuilder.AppendLine("    public void InitUI(GameObject uipanel)");
        strBuilder.AppendLine("    {");
        //如果初始化过就返回
        strBuilder.AppendLine("        if (isInit)");
        strBuilder.AppendLine("        {");
        strBuilder.AppendLine("            return;");
        strBuilder.AppendLine("        }");
        strBuilder.AppendLine("        isInit = true;");

        foreach (KeyValuePair<string, List<Component>> pair in Path2WidgetCachedDict)
        {
            foreach (var info in pair.Value)
            {
                Component widget = info;
                //目标路径
                string path = GetWidgetPath(widget.transform, transRoot);

                if (path.Contains(UIPanelPrefix) || path.Contains(JumpName))
                {
                    continue;
                }
                //组件type 名
                string strClassType = widget.GetType().ToString();
                //组件名
                string componentName = strClassType.Split('.').ToList().Last();
                //变量名
                string codeName = widget.name + componentName;

                strBuilder.AppendFormat("\t\t{0} = transform.Find(\"{1}\").GetComponent<{2}>();\r\n", codeName, path, componentName);

            }
        }
        strBuilder.AppendLine("        ");
        strBuilder.AppendLine("    }");
    }

    private static void FindAllWidgets(Transform trans, string strPath)
    {
        if (null == trans)
        {
            return;
        }
        for (int nIndex = 0; nIndex < trans.childCount; ++nIndex)
        {
            Transform child = trans.GetChild(nIndex);
            string strTemp = strPath + "/" + child.name;

            //生成子UI
            if (child.name.StartsWith(UIWidgetPrefix))
            {
                foreach (var uiComponent in WidgetInterfaceList)
                {
                    Component component = child.GetComponent(uiComponent);
                    if (null == component)
                    {
                        continue;
                    }

                    if (Path2WidgetCachedDict.ContainsKey(child.name))
                    {
                        Path2WidgetCachedDict[child.name].Add(component);
                        continue;
                    }

                    List<Component> componentsList = new List<Component>();
                    componentsList.Add(component);
                    Path2WidgetCachedDict.Add(child.name, componentsList);
                }
            }

            FindAllWidgets(child, strTemp);
        }
    }


    static UIItemCodeSpawner()
    {
        WidgetInterfaceList = new List<string>();
        WidgetInterfaceList.Add("ButtonExt");
        WidgetInterfaceList.Add("Text");
        WidgetInterfaceList.Add("TMPro.TextMeshProUGUI");
        WidgetInterfaceList.Add("InputField");
        WidgetInterfaceList.Add("Scrollbar");
        WidgetInterfaceList.Add("ToggleGroup");
        WidgetInterfaceList.Add("Toggle");
        WidgetInterfaceList.Add("Dropdown");
        WidgetInterfaceList.Add("Slider");
        WidgetInterfaceList.Add("ScrollRect");
        WidgetInterfaceList.Add("Image");
        WidgetInterfaceList.Add("RawImage");
        WidgetInterfaceList.Add("RectTransform");
        WidgetInterfaceList.Add("SlidingBlock");
        WidgetInterfaceList.Add("CanvasGroup");
    }


}