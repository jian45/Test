using UnityEngine;

public class AdNormalState : AdBaseState
{
    public override void OnEnter(AdPanel adPanel)
    {
        Debug.Log("进入 AdNormalState");
        this.adPanel = adPanel;
        adPanel.PauseAd = false;
    }

    public override void LogicUpdate()
    {
        if (adPanel.PauseAd) return;

        adPanel.AdCurrentTime -= Time.deltaTime;
        adPanel.Adprogress.value = adPanel.AdCurrentTime / adPanel.AdAllTime;

        if (adPanel.AdCurrentTime <= 0f)
        {
            adPanel.CompletedAd = true;
            adPanel.CompleteIcon.gameObject.SetActive(true);
            adPanel.SwitchState(AdSate.success);
        }
    }

    public override void PhysicsUpdate()
    {
    }

    public override void OnExit()
    {
    }
}
