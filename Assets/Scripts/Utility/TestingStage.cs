using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingStage : MonoBehaviour
{

   
    [SerializeField] private List<CatTableData> catTableList;
    [SerializeField] private bool OnCompletedOrderEvent;
    public bool CompletedOrder;
    [SerializeField] private voidEventSO CompletedOrderEvent ;
    
  public Vector3 catposition;
    [SerializeField] private int catID;
    [SerializeField] private Transform Environment;


    void Start()
    {
        SpawnCat(catID, catposition);
    }
    private void Update()
    {
     
        if ( CompletedOrder && !OnCompletedOrderEvent )
        {
           
            StartCompletedOrderEvent();
        }
    }
    private void StartCompletedOrderEvent()
    {
        OnCompletedOrderEvent = true;
        if (CompletedOrderEvent != null)
            CompletedOrderEvent.RaisedEvent();
        else
            Debug.LogWarning("TestingStage: CompletedOrderEvent 为空，无法广播事件");
    }


    public CatTableData GetCatTableData()
    {
        return catTableList[0];
    }

    public void SpawnCat(int catId, Vector3 position)
    {
        CatDataItem data = GetCatTableData().GetCatDataByID(catId);

        if (data == null)
        {
            Debug.LogError($"{catId}配置数据不存在，请检查猫咪配置表");
            return;
        }

        Debug.Log($"{catId}配置数据存在，加载猫咪{data.name}");

        if (string.IsNullOrEmpty(data.bodyAddress))
        {
            Debug.Log($" {data.name}(ID:{catId})的bodyAddress未赋值，请检查配置");
            return;
        }

        ABManager.Instance.InstantiatePrefab(
            data.bodyAddress,
            position,
            Quaternion.identity,
             Environment,
            (go) =>
            {
                if (go != null)
                    go.name = data.name;
                Debug.Log("实例化"+data.name);
            },
            (error) =>
            {
                Debug.LogError($"猫咪实例化失败: {error}");
            });
    }




}
