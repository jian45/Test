using UnityEngine;

public class AdSuccessState : AdBaseState
{
    public override void OnEnter(AdPanel adPanel)
    {
        Debug.Log("进入 AdSuccessState");
        this.adPanel = adPanel;
        adPanel.CompleteIcon.gameObject.SetActive(true);
    }

    public override void LogicUpdate()
    {
    }

    public override void PhysicsUpdate()
    {
    }

    public override void OnExit()
    {
    }
}
