using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Data/RewardTableData", fileName = "RewardTableData")]

public class ResultRewardData : ScriptableObject
{
    public List<RewardDataItem> DataList = new List<RewardDataItem>();

    // ����ʱ�ֵ仺�棬�״β�ѯʱ��ʼ��
    private Dictionary<int,RewardDataItem> _RewardDataDic;

    public RewardDataItem GetRewardDataByID(int id)
    {
        if (_RewardDataDic == null)
        {
            _RewardDataDic = new Dictionary<int, RewardDataItem>();
            foreach (var item in DataList)
            {
                if (!_RewardDataDic.ContainsKey(item.RewardId))
                    _RewardDataDic.Add(item.RewardId, item);
            }
        }
        _RewardDataDic.TryGetValue(id, out var data);
        return data;
    }
}
[System.Serializable]

public class RewardDataItem
{
    public int RewardId;

    public int RewardCatCoin;

    public int RewardHeart;

    public int RewardMaterial;

}