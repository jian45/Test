using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;
/// <summary>
/// 这是面板基类提供所有面板的共同方法与参数
/// 如：面板的淡入淡出，子类抽象初始化方法
/// </summary>
public abstract class BasePanel : MonoBehaviour
{
    //整体控制淡入淡出的画布组 组件
    protected CanvasGroup canvasGroup;
    //淡入淡出方法
    private float alphaSpeed = 10;
    //标识：是否开始显示
    private bool isShow;
    //当自己淡出成功时 要执行的委托函数（回调）
    private Action hideCallBank;
    protected virtual void Awake() 
    {
        //得到画布组 组件
        canvasGroup = this.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            //若是没有自行添加一个
            canvasGroup = this.gameObject.AddComponent<CanvasGroup>();
        }
    }
    protected virtual void Start()
    {
        Init();
    }
    /// <summary>
    /// 子类初始化方法，必须使用
    /// 处理事件按钮的监听
    /// </summary>
    protected abstract void Init();

    /// <summary>
    /// 显示自己时要做的事
    /// </summary>
    public virtual void ShowMe() 
    {
        isShow = true;
        canvasGroup.alpha = 0;
    }
    /// <summary>
    /// 隐藏自己时要做的事
    /// </summary>
    public virtual void HideMe(Action callbake) 
    {
        isShow = false;
        canvasGroup.alpha = 1;
        hideCallBank += callbake;
    }
    protected virtual void Update()
    {
        if (isShow && canvasGroup.alpha != 1)
        {
            canvasGroup.alpha += alphaSpeed * Time.deltaTime;
            if (canvasGroup.alpha >= 1)
            {
                canvasGroup.alpha = 1;
            }
        }
        //淡出
        else if (!isShow)
        {
            canvasGroup.alpha -= alphaSpeed * Time.deltaTime;
            if (canvasGroup.alpha <= 0)
            {
                canvasGroup.alpha = 0;
                //淡出成功后 删除自身
                hideCallBank?.Invoke();
                hideCallBank = null;
            }
        }
    }

}
