using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gift
{
    /// <summary>
    /// 类型
    /// </summary>
    public ProductType ItemType = ProductType.item;
    /// <summary>
    /// ID
    /// </summary>
    public int id = 0;
    /// <summary>
    /// 数量
    /// </summary>
    public double count = 0;
    /// <summary>
    /// 名称语言ID
    /// </summary>
    public double name = 0;
    /// <summary>
    /// 描述语言ID
    /// </summary>
    public double desc = 0;
    /// <summary>
    /// 背包道具类型（仅适用于背包类道具）
    /// </summary>
    public ItemGroup group = ItemGroup.未分类;

    /// <summary>
    /// 图标路径
    /// </summary>
    public string iconPath;
    /// <summary>
    /// 图标名
    /// </summary>
    public string iconName;
    /// <summary>
    /// 背景路径
    /// </summary>
    public string bgPath;
    /// <summary>
    /// 背景名（品质颜色）
    /// </summary>
    public string bgName;

    /// <summary>
    /// 道具是否存在/拥有配置  
    /// </summary>
    public bool isExisted = false;

    /// <summary>
    /// 通用
    /// </summary>
    public Gift(string itemStr)
    {
        if (itemStr.Contains(';'))
        {
            var str = itemStr.Split(';', System.StringSplitOptions.RemoveEmptyEntries);
            Init(str[0]);
        }
        else
        {
            Init(itemStr);
        }
    }

    
    private void Init(string itemStr)
    {
        var info = itemStr.Split("_");
        ItemType.value = info[0];
        count = info.Length > 2 ? GetRewardNum(info[2]) : 0;

        if (ItemType == ProductType.item)
        {
            if (ItemConfig.list.TryGetValue(double.Parse(info[1]), out var class_ItemConfig))
            {
                // 道具背包共同项
                id = (int)class_ItemConfig.id;
                group = (ItemGroup)(int)class_ItemConfig.group;
                name = class_ItemConfig.name;
                desc = class_ItemConfig.desc;
                iconName = class_ItemConfig.icon;
                bgName = class_ItemConfig.quality.ToString();
                isExisted = true;
                // 道具背包非共同项
                iconPath = "ItemIcon";
                bgPath = "ItemBg";
            }
            else //不存在的道具（如特殊id配表的随机宝石、log类型标记等）
            {
                int.TryParse(info[1], out int id);
                this.id = id;
            }
        }
    }

    /// <summary>
    /// 将数量格式转换为正确的数量
    /// </summary>
    /// <returns></returns>
    public static double GetRewardNum(string count)
    {
        if (count.Contains("-"))
        {
            string[] groupString = count.Split(new char[] { '-' });
            double groupElementCount = GetRewardNumByString(groupString[0]);
            int groupCount = int.Parse(groupString[1]);
            return groupElementCount * groupCount;
        }
        else
        {
            return GetRewardNumByString(count);
        }
    }
    public static double GetRewardNumByString(string count)
    {
        if (count.EndsWith("A"))
        {
            count = count.Replace("A", string.Empty);
            return double.Parse(count) * 1000;
        }
        else if (count.EndsWith("B"))
        {
            count = count.Replace("B", string.Empty);
            return double.Parse(count) * 1000000;
        }
        else if (count.EndsWith("C"))
        {
            count = count.Replace("C", string.Empty);
            return double.Parse(count) * 1000000000;
        }
        else if (count.EndsWith("D"))
        {
            count = count.Replace("D", string.Empty);
            return double.Parse(count) * 1000000000000;
        }
        else
        {
            return double.Parse(count);
        }
    }

    /// <summary>
    /// 转换为奖励字符串
    /// </summary>
    /// <returns></returns>
    public string VoToString()
    {
        return $"{ItemType}_{id}_{count}";
    }
}


/// <summary>
/// 道具类型  (野人的)
/// </summary>
public class ProductType
{
    public string value;

    private ProductType(string iValue)
    {
        value = iValue;
    }

    public static bool operator ==(ProductType lhs, ProductType rhs)
    {
        if (lhs.value == rhs.value)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool operator !=(ProductType lhs, ProductType rhs)
    {
        if (lhs.value != rhs.value)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override string ToString()
    {
        return value;
    }

    /// <summary>
    /// 物品
    /// </summary>
    public static ProductType item
    {
        get { return new ProductType("i"); }
    }
}

public enum ItemGroup
{
    未分类,
    货币,
}