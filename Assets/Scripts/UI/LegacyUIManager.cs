using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// UI控制器脚本负责实现不同面板的生成与销毁
/// 实现了单例模式、面板字典、显示面板方法、隐藏面板方法、获得面板方法
/// 我们不会将它挂载场景上
/// </summary>
public class LegacyUIManager 
{
    //单例
    private static LegacyUIManager instance =new LegacyUIManager();
    //全局访问点
    public static LegacyUIManager Instance => instance;
    //存储面板的容器（字典）
    //BasePanel之后可以使用里氏替换原则中的父类装子类来得到并存储不同的面板
    private Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();

    //一开始就应该得到父对象之后让面板动态生成时附着在canvas上
    private Transform canvas;

    private LegacyUIManager()
    {

        // 获取场景中 Canvas 的 Transform 组件，作为动态生成面板的父对象
        canvas = GameObject.Find("Canvas").transform;
        //过场不删canvas依附的对象
        //我们通过 动态创建 和动态删除来显示与隐藏面板
        GameObject.DontDestroyOnLoad(canvas.gameObject);
    }
    //显示面板
    //使用泛型函数，后面显现什么面板就写什么面板，约束是必须继承BasePanel类
    //ABManager 是异步加载，不能用同步返回，所以改回调模式
    //参数onSuccess不同于ABManager的onSuccess，这里使用时要用 Lambda 表达式使用对应面板内容
    public void ShowPanel<T>(Action<T> onSuccess = null) where T : BasePanel
    {
        //我们需要保证 泛型类型T的名字 和面板声明的一致
        string panelName = typeof(T).Name;
        //我们要显示一个面板如果这个面板已经显示了我们就不用生成新的面板
        if (panelDic.TryGetValue(panelName , out BasePanel existing))
        {
            onSuccess?.Invoke(existing  as T );
            return;
        }

        //显示面板 就是 动态的创建面板预制体 设置父对象
        //根据得到的 类名 就是我们的预设体面板明 直接 动态创建它 即可
       
        //预制体文件位置
        string address = "Prefab/Panel/" + panelName ;
        ABManager.Instance.InstantiatePrefab(
           address,
           Vector3.zero,
           Quaternion.identity,
           null,
           onSuccess: instance =>
           {
              
               //设置父对象
               instance.transform.SetParent(canvas, false);
             //接着就是得到对应的面板脚本 存储起来
                T panle = instance.GetComponent<T>();
                //把面板存储到对应的容器中 方便我们获取它
                panelDic.Add(panelName, panle);
                onSuccess?.Invoke(panle);
           },
           onFailed: error =>
           {
               Debug.LogError($"加载失败: {error}");
           }

           );
       
       
    }

    //隐藏面板
    //参数一:如果希望 淡出 就默认传true如果希望直接隐藏(删除)面板 那么就传false
    public void HidePanel<T>(bool isFade = true) where T : BasePanel 
    {
      
        //根据泛型类型得到 面板名字
        string PanelName = typeof (T).Name;
        //再通过名字得到字典中的对应面板

        //判断字典中是否有这个面板脚本
        if (panelDic .ContainsKey(PanelName)) 
        {
            if (isFade)
            {
                //淡出
                // lambda表达式 代表一个匿名函数 这里的匿名函数就是淡出成功后的回调函数
                panelDic[PanelName].HideMe(() =>
                {
                    //面板 淡出成功后 希望删除面板
                    GameObject.Destroy(panelDic[PanelName].gameObject);
                    //删除后从字典中删除
                    panelDic.Remove(PanelName);
                });
            }
            else 
            {
                //不淡出则直接删除
                GameObject.Destroy(panelDic[PanelName].gameObject);
                //删除后从字典中删除
                panelDic.Remove(PanelName);
            }
        }
    }
    //获得面板
    public T GetPanel<T>() where T : BasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            return panelDic[panelName] as T;
        }
        //如果没有 直接返回空
        return null;
    }
}
