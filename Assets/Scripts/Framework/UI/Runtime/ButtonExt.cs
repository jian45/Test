using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ClientFramework.UI
{
    public class ButtonExt : Button
    {
        [SerializeField] private bool enableLongPress;
        [SerializeField] private float longPressTime = 0.45f;
        [SerializeField] private bool repeatLongPress;
        [SerializeField] private float repeatInterval = 0.12f;
        [SerializeField] private bool suppressClickAfterLongPress = true;
        [SerializeField] private float clickCooldown = 0.2f;
        [SerializeField] private RectTransform visualRoot;
        [SerializeField] private float pressedScale = 0.94f;
        [SerializeField] private float tweenDuration = 0.08f;

        public UnityEvent onLongPress = new UnityEvent();
        public UnityEvent onLongPressRepeat = new UnityEvent();

        private Coroutine longPressCoroutine;
        private bool pointerHeld;
        private bool suppressNextClick;
        private float nextClickAllowedTime;

        public void AddClick(UnityAction action)
        {
            if (action == null) return;
            this.onClick.AddListener(action);
        }

        public void AddLongPress(UnityAction action)
        {
            if (action == null) return;
            this.onLongPress.AddListener(action);
        }

        public void RemoveClick(UnityAction action)
        {
            if (action == null) return;
            this.onClick.RemoveListener(action);
        }

        public void RemoveLongPress(UnityAction action)
        {
            if (action == null) return;
            this.onLongPress.RemoveListener(action);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (!IsActive() || !IsInteractable())
                return;

            PlayScale(pressedScale);

            if (pointerHeld)
                return;

            pointerHeld = true;
            suppressNextClick = false;

            if (enableLongPress && longPressCoroutine == null)
                longPressCoroutine = StartCoroutine(TrackLongPress());
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            StopLongPress(false);
            PlayScale(1f);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            StopLongPress(false);
            PlayScale(1f);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (suppressNextClick)
            {
                suppressNextClick = false;
                return;
            }

            if (!IsActive() || !IsInteractable())
                return;

            float safeCooldown = Mathf.Max(0f, clickCooldown);
            float now = Time.unscaledTime;
            if (safeCooldown > 0f && now < nextClickAllowedTime)
                return;

            nextClickAllowedTime = now + safeCooldown;
            base.OnPointerClick(eventData);
        }

        protected override void OnDisable()
        {
            ResetPressImmediately();
            base.OnDisable();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                ResetPressImmediately();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                ResetPressImmediately();
        }

        private IEnumerator TrackLongPress()
        {
            float safeLongPressTime = Mathf.Max(0f, longPressTime);
            if (safeLongPressTime <= 0f)
            {
                yield return null;
            }
            else
            {
                float elapsed = 0f;
                while (pointerHeld && elapsed < safeLongPressTime)
                {
                    yield return null;
                    elapsed += Time.unscaledDeltaTime;
                }
            }

            if (!pointerHeld)
            {
                longPressCoroutine = null;
                yield break;
            }

            if (suppressClickAfterLongPress)
                suppressNextClick = true;

            onLongPress?.Invoke();

            if (!repeatLongPress)
            {
                longPressCoroutine = null;
                yield break;
            }

            while (pointerHeld && repeatLongPress)
            {
                float safeRepeatInterval = Mathf.Max(0f, repeatInterval);
                if (safeRepeatInterval <= 0f)
                {
                    yield return null;
                }
                else
                {
                    float elapsed = 0f;
                    while (pointerHeld && repeatLongPress &&
                           elapsed < safeRepeatInterval)
                    {
                        yield return null;
                        elapsed += Time.unscaledDeltaTime;
                    }
                }

                if (pointerHeld && repeatLongPress)
                    onLongPressRepeat?.Invoke();
            }

            longPressCoroutine = null;
        }

        private void StopLongPress(bool clearPendingClick)
        {
            pointerHeld = false;

            if (longPressCoroutine != null)
            {
                StopCoroutine(longPressCoroutine);
                longPressCoroutine = null;
            }

            if (clearPendingClick)
                suppressNextClick = false;
        }

        private void PlayScale(float scale)
        {
            if (visualRoot == null)
                return;

            visualRoot.DOKill();

            Vector3 targetScale =
                Vector3.one * Mathf.Max(0f, scale);
            float safeDuration = Mathf.Max(0f, tweenDuration);
            if (safeDuration <= 0f)
            {
                visualRoot.localScale = targetScale;
                return;
            }

            visualRoot
                .DOScale(targetScale, safeDuration)
                .SetUpdate(true);
        }

        private void ResetPressImmediately()
        {
            bool interruptedPress =
                pointerHeld || longPressCoroutine != null;
            StopLongPress(false);
            suppressNextClick =
                interruptedPress || suppressNextClick;

            if (visualRoot == null)
                return;

            visualRoot.DOKill();
            visualRoot.localScale = Vector3.one;
        }
    }
}
