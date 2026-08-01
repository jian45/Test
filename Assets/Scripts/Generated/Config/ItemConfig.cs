using System;
using System.Collections.Generic;
public class Class_ItemConfig
{
    /// <summary>
    /// id
    /// </summary>
    public double id;
    /// <summary>
    /// 名称ID
    /// </summary>
    public double name;
    /// <summary>
    /// 介绍文案ID
    /// </summary>
    public double desc;
    /// <summary>
    /// 图标素材名
    /// </summary>
    public string icon;
    /// <summary>
    /// 品质
    /// </summary>
    public double quality;
    /// <summary>
    /// 道具分组
    /// </summary>
    public double group;
    /// <summary>
    /// 使用类型 0=背包内不可使用 1=允许单个使用 2=允许多个使用
    /// </summary>
    public double useType;
    /// <summary>
    /// 掉落id
    /// </summary>
    public double dropid;
}

public class ItemConfig
{
    public static  Dictionary<double,Class_ItemConfig> list = new Dictionary<double, Class_ItemConfig>();
    static void ItemConfig0()
    {
        Class_ItemConfig tem2 = new Class_ItemConfig();
        tem2.id = 1001;
        tem2.name = 20241;
        tem2.desc = 20088;
        tem2.icon = "猫币";
        tem2.quality = 1;
        tem2.group = 1;
        tem2.useType = 0;
        tem2.dropid = 0;
        list.Add(tem2.id,tem2);

    }
    static ItemConfig()
    {
        ItemConfig0();
    }
}
