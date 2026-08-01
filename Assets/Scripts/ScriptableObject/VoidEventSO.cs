using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(menuName = "Event/voidEventSO")]

public class voidEventSO : ScriptableObject
{
    public UnityAction OnEventRaised;
    public void RaisedEvent()
    {
        OnEventRaised?.Invoke();
    }



}
