using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Tools.Utility;
using UnityEngine;

public class CustomerFeedback : MonoBehaviour
{
    
    [SerializeField] private float DelayCounter;

    [Header("广播")]
    [SerializeField] private voidEventSO CompletedFeedbackEvent;

    [Header("监听")]
    [SerializeField] private voidEventSO CustomerFeedbackEventSO;

    
    public float strength = 0.3f;

    public float duration = 0.5f;
   
    public float speed = 25f;

    private Tween delayTween;
    private Tween shakeTween;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnEnable()
    {
        if (CustomerFeedbackEventSO != null)
            CustomerFeedbackEventSO.OnEventRaised += Shake;
        else
            Debug.LogWarning("CustomerFeedback: CustomerFeedbackEventSO 为空，未订阅事件");
    }
    private void OnDisable()
    {
        if (CustomerFeedbackEventSO != null)
            CustomerFeedbackEventSO.OnEventRaised -= Shake;
        else
            Debug.LogWarning("CustomerFeedback: CustomerFeedbackEventSO 为空，无法取消订阅");
    }

    public void Shake()
    {
        DOTweenUtil.Kill(delayTween);
        DOTweenUtil.Kill(shakeTween);
        delayTween = DOTweenUtil.Delay(DelayCounter, DoShake, target: this);
    }

    private void DoShake()
    {
        shakeTween = DOTweenUtil.ShakePosition(
            transform,
            duration: duration,
            strength: strength,
            vibrato: (int)speed,
            onComplete: () =>
            {
                if (CompletedFeedbackEvent != null)
                    CompletedFeedbackEvent.RaisedEvent();
                else
                    Debug.LogWarning("CustomerFeedback: CompletedFeedbackEvent 为空，无法广播事件");
            }
        );
    }
}