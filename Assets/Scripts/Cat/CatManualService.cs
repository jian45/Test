using UnityEngine;

public class CatManualService : CatBaseState
{
    private Camera cam;
    private Vector3 dragOffset;
    private Plane dragPlane;
    private bool hasPlane;

    public CatManualService(Cat cat) : base(cat) { }

    public override void OnEnter()
    {
        Debug.Log("进入 ManualService");
        currentCat.Rb.freezeRotation = true;
        currentCat.Rb.bodyType = RigidbodyType2D.Kinematic;

        cam = Camera.main;
        if (cam == null) return;

        Vector3 catPos = currentCat.transform.position;
        Vector3 planeNormal = Vector3.back;
        dragPlane = new Plane(planeNormal, catPos);
        hasPlane = true;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float dist))
            dragOffset = catPos - ray.GetPoint(dist);
        else
            dragOffset = Vector3.zero;
    }

    public override void LogicUpdate()
    {
        if (!hasPlane || cam == null) return;

        if (Input.GetMouseButtonUp(0))
        {
            CheckDistance();
            return;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float dist))
        {
            Vector3 hitPoint = ray.GetPoint(dist);
            currentCat.Rb.MovePosition(hitPoint + dragOffset);
        }
    }

    private void CheckDistance()
    {
        if (Mathf.Abs(currentCat.CurrentCustomerDist) > 2 || Mathf.Abs(currentCat.CurrentCustomerDistY) > 2)
        {
            currentCat.SwitchState(CatSate.NormalState);
            return;
        }
        currentCat.Shake();
        if (currentCat.FeedbackEvent != null)
            currentCat.FeedbackEvent.RaisedEvent();
        else
            Debug.LogWarning("CatManualService: FeedbackEvent 为空，无法广播事件");
        currentCat.SwitchState(CatSate.NormalState);
    }

    public override void PhysicsUpdate()
    {
    }

    public override void OnExit()
    {
        currentCat.Rb.bodyType = RigidbodyType2D.Dynamic;
    }
}
