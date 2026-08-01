using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MergeBoard : MonoBehaviour
{
    public Button btn;//生成按钮
    public Text text;//提示文本
    [Tooltip("棋盘格子总数")]
    public int celllCount = 16;
    public int columns = 4;//列表
    [Tooltip("棋盘格颜色")]
    public Color colorA = Color.white;
    public Color colorB = Color.black;

    public void Start()
    {
        Generate();
    }

    public void Generate()
    {
        Clear();
        for (int i = 0; i < celllCount; i++) 
        {
        GameObject cell = new GameObject("cell",typeof(RectTransform),typeof(Image));
            cell.transform.SetParent(transform, false);
            Image img = cell.GetComponent<Image>();
            img.color =(i/columns+i%columns)%2==0 ? colorA : colorB;
        }
    }

    public void Clear()
    {List<GameObject> toDestroy = new List<GameObject>();
        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject chid = transform.GetChild(i).gameObject;
            if (chid.name == "cell")
            {
                toDestroy.Add(chid);
            }
        foreach(GameObject obj in toDestroy)DestroyImmediate(obj);
        }
          
    }
}
