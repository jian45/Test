using UnityEngine;

public class CatNormalState : CatBaseState
{
    private float timer;
    private const float TIMEOUT = 3f;
    private bool isWaiting;

    public CatNormalState(Cat cat) : base(cat) { }

    public override void OnEnter()
    {
        Debug.Log("进入 NormalState");
        isWaiting = false;
    }

    public override void LogicUpdate()
    {
        if (!isWaiting) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isWaiting = false;
            currentCat.SwitchState(CatSate.AutoService);
        }
    }

    public override void OnOrderCompleted()
    {
        isWaiting = true;
        timer = TIMEOUT;
    }

    public override void PhysicsUpdate()
    {
    }

    public override void OnExit()
    {
    }
}
