using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/CatTableData", fileName = "CatTableData")]

public class CatTableData : ScriptableObject
{

    public List<CatDataItem> DataList = new List<CatDataItem>();


    private Dictionary<int, CatDataItem> _catDataDic;

    public CatDataItem GetCatDataByID(int id)
    {
        if (_catDataDic == null)
        {
            _catDataDic = new Dictionary<int, CatDataItem>();
            foreach (var item in DataList)
            {
                if (!_catDataDic.ContainsKey(item.id))
                    _catDataDic.Add(item.id, item);
            }
        }
        _catDataDic.TryGetValue(id, out var data);
        return data;
    }
}

[System.Serializable]
    public class CatDataItem
    {

        public int id;

        public string uid;

        public string name;

        public float speed;

        public float intimacy;

        public string description;

        public string bodyAddress;

         public Sprite headIcon;
}

