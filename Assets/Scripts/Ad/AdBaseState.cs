public abstract class AdBaseState
{
    protected AdPanel adPanel;

    public abstract void OnEnter(AdPanel adPanel);
    public abstract void LogicUpdate();
    public abstract void PhysicsUpdate();
    public abstract void OnExit();

    public virtual void HandleRetry() { }
}
