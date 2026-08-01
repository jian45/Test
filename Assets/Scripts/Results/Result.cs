using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Result : MonoBehaviour
{
    [Header("��������")]
    
    [SerializeField] private int BasicBonusValue;
    [SerializeField] private int AllCoinValue;
    [SerializeField] private Transform ResultsBasicBonus;
    [SerializeField] private Transform ResultsAllCoin;
    [SerializeField] private Transform coinbonusManager;
    // Start is called before the first frame update
    void Start()
    {

        ResultsAllCoin = transform.Find("AllCoin/ResultsAllCoin");
        ResultsBasicBonus = transform.Find("Bonus/ResultsBonus");
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
        if (ResultsAllCoin != null && coinbonusManager != null)
        {
            CoinBonusManager cbm = coinbonusManager.GetComponent<CoinBonusManager>();
            if (cbm != null)
                BasicBonusValue = cbm.BasicBonus;
            Text AllCoinText = ResultsAllCoin.GetComponent<Text>();
            if (AllCoinText != null)
            {
                AllCoinText.text = $"����������{BasicBonusValue}";
            }
        }
    }
    private void BasicBonusUpdate()
    {
        if (ResultsBasicBonus != null)
        {
            AllCoinValue = PlayerResources.Instance.CatCoins;
            Text BasicBonusText = ResultsBasicBonus.GetComponent<Text>();
            if (BasicBonusText != null)
            {
                BasicBonusText.text = $"ȫ���ʽ�{AllCoinValue + BasicBonusValue}";
            }
        }
    }
}
