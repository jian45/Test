using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBonusManager : MonoBehaviour
{
    public int BasicBonus;
    public int CurrentCoin;

    private void Start()
    {
        //¸³Öµ
        PlayerResources.Instance.AddCatCoins(CurrentCoin);
        
    }


}
