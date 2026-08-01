using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Coin : MonoBehaviour
{
   

    [Header("��������")]
    [SerializeField] private int BasicBonusValue;
    [SerializeField] private int AllCoinValue;
    [SerializeField] private Transform BasicBonus;
    [SerializeField] private Transform AllCoin;
    [SerializeField] private Transform coinbonusManager;
    // Start is called before the first frame update
    void Start()
    {

        AllCoin = transform.Find("AllCoin");
        BasicBonus = transform.Find("BasicBonus");
        GameObject coinBonusGO = GameObject.FindWithTag("CoinBonusManager");
        if (coinBonusGO != null)
            coinbonusManager = coinBonusGO.transform;
    }

    // Update is called once per frame
    void Update()
    {
        AllCoinUpdate();
        BasicBonusUpdate();
    }
    private void AllCoinUpdate()
    {
        if (AllCoin != null)
        {
            AllCoinValue = PlayerResources.Instance.CatCoins;
            
            Text AllCoinText = AllCoin.GetComponent<Text>();
            if (AllCoinText != null)
            {
                AllCoinText.text = $"ȫ��è�ң�{AllCoinValue}";
            }
        }
    }
    private void BasicBonusUpdate()
    {
        if (BasicBonus != null && coinbonusManager != null)
        {
            CoinBonusManager cbm = coinbonusManager.GetComponent<CoinBonusManager>();
            if (cbm != null)
                BasicBonusValue = cbm.BasicBonus;
            Text BasicBonusText = BasicBonus.GetComponent<Text>();
            if (BasicBonusText != null)
            {
                BasicBonusText.text = $"����������{BasicBonusValue}";
            }
        }
    }
}
