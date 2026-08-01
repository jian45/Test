using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物品字符串反序化后的实体类
/// 可用于物品字符串序列化和反序列化，会自动合并相同种类的物品
/// </summary>
public class ListGift
{
    public static ListGift FromString(string str)
    {
        var arrDataList = str.Split(';');  

        ListGift listGift = new ListGift();
        for (int i = 0; i < arrDataList.Length; i++)
        {
            if (arrDataList[i] != "")
            {
                Gift gift = new Gift(arrDataList[i]);
                listGift.Merge(gift);
            }
        }
        return listGift;
    }
    public static List<ListGift>
        ListGiftFormString(string str)
    {
        var arrDataList = str.Split(';');  //b_1_1

        List<ListGift> listGiftList = new List<ListGift>();
        for (int i = 0; i < arrDataList.Length; i++)
        {
            if (arrDataList[i] != "")
            {
                ListGift listGift = new ListGift();
                Gift gift = new Gift(arrDataList[i]);
                listGift.Merge(gift);
                listGiftList.Add(listGift);
            }
        }
        return listGiftList;
    }
    public void FromRewardString(string str)
    {
        var arrDataList = str.Split(';');  //b_1_1
        for (int i = 0; i < arrDataList.Length; i++)
        {
            if (arrDataList[i] != "")
            {
                Gift gift = new Gift(arrDataList[i]);
                Merge(gift);
            }
        }
    }

    public List<Gift> list = new List<Gift>();

    /// <summary>
    /// 添加物品
    /// </summary>
    /// <param name="productcode">格式：i_1</param>
    /// <param name="count">格式：1</param>
    public void Add(string productcode, string count)
    {
        string[] arr = productcode.Split('_');
        string itemType = arr[0].Trim(); //i
        string itemId = arr[1].Trim();//1
        Add(itemType, itemId, count);
    }

    /// <summary>
    /// 添加物品
    /// </summary>
    /// <param name="productType">格式：i</param>
    /// <param name="productid">格式：1</param>
    /// <param name="count">格式：1</param>
    public void Add(string productType, int productid, long count)
    {
        Gift gift = new Gift($"{productType}_{productid}_{count}");
        Merge(gift);
    }

    /// <summary>
    /// 添加物品
    /// </summary>
    /// <param name="productType">格式：i</param>
    /// <param name="productid">格式：1</param>
    /// <param name="count">格式：1</param>
    public void Add(string productType, string productid, string count)
    {
        Gift gift = new Gift($"{productType}_{productid}_{count}");
        Merge(gift);
    }

    public string ToRewardString()
    {
        string rewardInfo = "";
        for (int i = 0; i < list.Count; i++)
        {
            var itemVo = list[i];
            string itemType = itemVo.ItemType.value; //b
            int itemId = itemVo.id;//1
            string itemNum = itemVo.count.ToString();//1
            rewardInfo += itemType + "_" + itemId + "_" + itemNum + ";";
        }
        return rewardInfo;
    }
    /// <summary>
    /// 转换为奖励乘以Mutiple倍数的字符串(不改变自己的数据)
    /// </summary>
    /// <param name="mutiple"></param>
    /// <returns></returns>
    public string ToMutipleRewardString(float mutiple)
    {
        string rewardInfo = "";
        for (int i = 0; i < list.Count; i++)
        {
            var itemVo = list[i];
            string itemType = itemVo.ItemType.value; //b
            int itemId = itemVo.id;//1
            double itemNum = itemVo.isExisted ? Math.Ceiling(itemVo.count * mutiple) : itemVo.count;
            rewardInfo += itemType + "_" + itemId + "_" + itemNum + ";";
        }
        return rewardInfo;
    }

    public void Merge(Gift gift)
    {
        if (gift == null || gift.id == 0) return;
        for (int i = 0; i < list.Count; i++)
        {
            var curGift = list[i];
            if (curGift.ItemType == gift.ItemType)
            {
                if (curGift.id == gift.id)
                {
                    curGift.count += gift.count;
                    return;
                }
            }
        }
        list.Add(gift);
    }

    public void Merge(ListGift listGift)
    {
        if (listGift == null) return;
        for (int i = 0; i < listGift.list.Count; i++)
        {
            var curGift = listGift.list[i];
            bool hasMerge = false;
            for (int j = 0; j < list.Count; j++)
            {
                var gift = list[j];
                if (gift.ItemType == curGift.ItemType)
                {
                    if (gift.id == curGift.id)
                    {
                        gift.count += curGift.count;
                        hasMerge = true;
                        break;
                    }
                }
            }
            if (!hasMerge)
            {
                list.Add(curGift);
            }
        }
    }
}
