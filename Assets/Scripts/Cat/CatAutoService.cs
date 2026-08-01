using UnityEngine;

public class CatAutoService : CatBaseState
{
    public CatAutoService(Cat cat) : base(cat) { }

    public override void OnEnter()
    {
        Debug.Log("进入 AutoService");
    }

    public override void LogicUpdate()
    {
        if (Mathf.Abs(currentCat.CurrentCustomerDist) <= 2f)
        {
            currentCat.Rb.velocity = Vector2.zero;
            ServeCustomer();
            return;
        }

        StartMove();
    }

    public override void PhysicsUpdate()
    {
    }

    public override void OnExit()
    {
    }

    /// <summary>
    /// 向顾客方向移动
    /// </summary>
    private void StartMove()
    {
        currentCat.Rb.velocity = new Vector2(
            currentCat.CatSpeed * -currentCat.FaceDir.x * Time.fixedDeltaTime,
            currentCat.Rb.velocity.y
        );
    }

    /// <summary>
    /// 到达顾客身边：播放抖动反馈、触发事件、切回 NormalState
    /// </summary>
    private void ServeCustomer()
    {
        currentCat.Shake();
        if (currentCat.FeedbackEvent != null)
            currentCat.FeedbackEvent.RaisedEvent();
        else
            Debug.LogWarning("CatAutoService: FeedbackEvent 为空，无法广播事件");
        currentCat.SwitchState(CatSate.NormalState);
    }
}
