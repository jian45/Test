
public abstract class CatBaseState
{
    protected readonly Cat currentCat;

    protected CatBaseState(Cat cat)
    {
        currentCat = cat;
    }

    public abstract void OnEnter();

    public abstract void LogicUpdate();
    public abstract void PhysicsUpdate();
    public abstract void OnExit();

    public virtual void OnOrderCompleted() { }
}
