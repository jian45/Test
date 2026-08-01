using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CatDataHolder : MonoBehaviour
{
    [Header("猫咪数据")]
    public string catId = "cat_001_tabby";
    public string catName = "初始狸花猫";
    [TextArea(2, 4)]
    public string catDescription = "“一只调皮的狸花猫，\n正在等你带它回家”";

    [Header("UI 绑定")]
    public Image catAvatarImage;
    public Text catNameText;
    public Text catDescText;
    public Button adoptButton;

    void Start()
    {
        ApplyData();
    }

    public void ApplyData()
    {
        if (catNameText != null) catNameText.text = catName;
        if (catDescText != null) catDescText.text = catDescription;
    }

    public void SetCatData(string newId, string newName, string newDesc)
    {
        catId = newId;
        catName = newName;
        catDescription = newDesc;
        ApplyData();
    }
}