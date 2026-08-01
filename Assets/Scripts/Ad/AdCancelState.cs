using UnityEngine;

public class AdCancelState : AdBaseState
{
    public override void OnEnter(AdPanel adPanel)
    {
        Debug.Log("进入 AdCancelState");
        this.adPanel = adPanel;
        adPanel.PauseAd = true;
        adPanel.ComfirmPage.gameObject.SetActive(true);
    }

    public override void LogicUpdate()
    {
    }

    public override void PhysicsUpdate()
    {
    }

    public override void OnExit()
    {
        adPanel.ComfirmPage.gameObject.SetActive(false);
    }
}
