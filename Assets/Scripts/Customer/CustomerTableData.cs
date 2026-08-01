using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Data/CustomerTableData", fileName = "CustomerTableData")]

public class CustomerTableData : ScriptableObject
{
    public List<CustomerDataItem> DataList = new List<CustomerDataItem>();

    // 运行时字典缓存，首次查询时初始化
    private Dictionary<int,CustomerDataItem> _customerDataDic;

}
[System.Serializable]

public class CustomerDataItem
{

    public int id;

    public string uid;

    public string name;

    public string description;

    public GameObject body;

    public Sprite headIcon;
}