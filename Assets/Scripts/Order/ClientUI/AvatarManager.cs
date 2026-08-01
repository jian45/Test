using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class AvatarManager : MonoBehaviour
{
    //单例模式
    private static AvatarManager instance;
    public static AvatarManager Instance=> instance;

    [Header("拖入 Sprite Atlas 资源")]
    public SpriteAtlas ClientAvatarAtlas;
    void Awake() 
    {
        instance = this;
    }


    /// <summary>根据顾客名获取头像 Sprite</summary>

    public Sprite  GetSprite(string spriteName) 
    {
    if (instance ==null||instance.ClientAvatarAtlas == null)
        {
            Debug.LogError("ClientAvatarAtlas 未赋值");
            return null;
        }
        Sprite sprite = ClientAvatarAtlas.GetSprite(spriteName);
        if (sprite == null)
        {
            Debug.LogError($"AvatarManager: 图集中找不到 {spriteName}");
        }
        return sprite;
    } 
}
