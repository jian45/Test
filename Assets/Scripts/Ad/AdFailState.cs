using UnityEngine;

public class AdFailState : AdBaseState
{
    private float delayTimer;
    private const float DELAY = 2f;
    private bool hasShownError;

    public override void OnEnter(AdPanel adPanel)
    {
        Debug.Log("进入 AdFailState");
        this.adPanel = adPanel;
        adPanel.PauseAd = true;
        adPanel.AdErrorPage.gameObject.SetActive(false);
        delayTimer = 0f;
        hasShownError = false;
    }

    public override void LogicUpdate()
    {
        delayTimer += Time.deltaTime;
        if (delayTimer >= DELAY && !hasShownError)
        {
            hasShownError = true;
            adPanel.AdErrorPage.gameObject.SetActive(true);
        }
    }

    public override void HandleRetry()
    {
        adPanel.AdErrorPage.gameObject.SetActive(false);
        delayTimer = 0f;
        hasShownError = false;
    }

    public override void PhysicsUpdate()
    {
    }

    public override void OnExit()
    {
    }
}
