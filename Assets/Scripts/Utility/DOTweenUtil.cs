using System;
using UnityEngine;
using DG.Tweening;

namespace Tools.Utility
{
    /// <summary>
    /// DOTween 通用工具类
    /// </summary>
    public static class DOTweenUtil
    {
        /// <summary>
        /// 默认动画时长
        /// </summary>
        public const float DefaultDuration = 0.25f;

        /// <summary>
        /// 默认 Punch 强度
        /// </summary>
        public static readonly Vector3 DefaultPunchScale = new Vector3(0.12f, 0.12f, 0f);


        #region 基础方法

        /// <summary>
        /// 判断 Tween 是否处于有效状态
        /// </summary>
        public static bool IsActive(Tween tween)
        {
            return tween != null && tween.IsActive();
        }
        
        
        /// <summary>
        /// 判断 Tween 是否正在播放
        /// </summary>
        public static bool IsPlaying(Tween tween)
        {
            return tween != null && tween.IsActive() && tween.IsPlaying();
        }
        
        
        /// <summary>
        /// 安全 Kill 一个 Tween，并将引用置空
        /// </summary>
        /// <param name="tween">Tween 引用</param>
        /// <param name="complete">Kill 前是否强制完成动画</param>
        public static void Kill(ref Tween tween, bool complete = false)
        {
            if (tween != null && tween.IsActive())
                tween.Kill(complete);

            tween = null;
        }
        
        
        /// <summary>
        /// 安全 Kill 一个 Tween，但不处理引用置空
        /// </summary>
        public static void Kill(Tween tween, bool complete = false)
        {
            if (tween != null && tween.IsActive())
                tween.Kill(complete);
        }
        
        
        /// <summary>
        /// Kill 指定对象作为 Target 的所有 Tween
        /// </summary>
        public static int KillTarget(UnityEngine.Object target, bool complete = false)
        {
            if (target == null)
                return 0;

            return DOTween.Kill(target, complete);
        }
        
        
        /// <summary>
        /// 暂停指定对象作为 Target 的所有 Tween
        /// </summary>
        public static int PauseTarget(UnityEngine.Object target)
        {
            if (target == null)
                return 0;

            return DOTween.Pause(target);
        }
        
        
        /// <summary>
        /// 恢复指定对象作为 Target 的所有 Tween
        /// </summary>
        public static int PlayTarget(UnityEngine.Object target)
        {
            if (target == null)
                return 0;

            return DOTween.Play(target);
        }
        
        
        /// <summary>
        /// 重新播放指定对象作为 Target 的所有 Tween
        /// </summary>
        public static int RestartTarget(UnityEngine.Object target)
        {
            if (target == null)
                return 0;

            return DOTween.Restart(target);
        }
        
        
        /// <summary>
        /// 创建延迟调用
        /// </summary>
        /// <param name="delay">延迟时间</param>
        /// <param name="callback">回调</param>
        /// <param name="ignoreTimeScale">是否忽略 Time.timeScale</param>
        /// <param name="target">可选 Target，用于后续 DOTween.Kill(target)</param>
        public static Tween Delay(
            float delay,
            Action callback,
            bool ignoreTimeScale = false,
            UnityEngine.Object target = null)
        {
            Tween tween = DOVirtual.DelayedCall(
                delay,
                () => callback?.Invoke(),
                ignoreTimeScale
            );

            if (target != null)
            {
                tween.SetTarget(target);

                GameObject linkGo = ResolveGameObject(target);
                if (linkGo != null)
                {
                    tween.SetLink(linkGo, LinkBehaviour.KillOnDestroy);
                }
            }

            return tween;
        }
        
        
        /// <summary>
        /// float 数值过渡
        /// </summary>
        public static Tweener ValueTo(
            float from,
            float to,
            float duration,
            Action<float> onUpdate,
            Ease ease = Ease.OutQuad,
            bool ignoreTimeScale = false,
            UnityEngine.Object target = null,
            Action onComplete = null)
        {
            float value = from;

            Tweener tweener = DOTween.To(
                () => value,
                x =>
                {
                    value = x;
                    onUpdate?.Invoke(x);
                },
                to,
                duration
            );

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }
        
        
        /// <summary>
        /// int 数值过渡
        /// </summary>
        public static Tweener ValueTo(
            int from,
            int to,
            float duration,
            Action<int> onUpdate,
            Ease ease = Ease.OutQuad,
            bool ignoreTimeScale = false,
            UnityEngine.Object target = null,
            Action onComplete = null)
        {
            int value = from;

            Tweener tweener = DOTween.To(
                () => value,
                x =>
                {
                    value = x;
                    onUpdate?.Invoke(x);
                },
                to,
                duration
            );

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }

        #endregion        
        
        
        #region Transform 移动动画

        /// <summary>
        /// 世界坐标移动
        /// </summary>
        public static Tweener MoveTo(
            Transform target,
            Vector3 endPosition,
            float duration = DefaultDuration,
            Ease ease = Ease.OutQuad,
            bool replaceOld = true,
            bool ignoreTimeScale = false,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target.DOMove(endPosition, duration);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }
        
        
        /// <summary>
        /// 本地坐标移动
        /// </summary>
        public static Tweener LocalMoveTo(
            Transform target,
            Vector3 endLocalPosition,
            float duration = DefaultDuration,
            Ease ease = Ease.OutQuad,
            bool replaceOld = true,
            bool ignoreTimeScale = false,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target.DOLocalMove(endLocalPosition, duration);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }
        
        
        /// <summary>
        /// 世界坐标 X 轴移动
        /// </summary>
        public static Tweener MoveX(
            Transform target,
            float endX,
            float duration = DefaultDuration,
            Ease ease = Ease.OutQuad,
            bool replaceOld = true,
            bool ignoreTimeScale = false,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target.DOMoveX(endX, duration);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }
        
        
        /// <summary>
        /// 世界坐标 Y 轴移动
        /// </summary>
        public static Tweener MoveY(
            Transform target,
            float endY,
            float duration = DefaultDuration,
            Ease ease = Ease.OutQuad,
            bool replaceOld = true,
            bool ignoreTimeScale = false,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target.DOMoveY(endY, duration);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }
        
        
        /// <summary>
        /// 本地坐标 X 轴移动
        /// </summary>
        public static Tweener LocalMoveX(
            Transform target,
            float endX,
            float duration = DefaultDuration,
            Ease ease = Ease.OutQuad,
            bool replaceOld = true,
            bool ignoreTimeScale = false,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target.DOLocalMoveX(endX, duration);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }

        
        /// <summary>
        /// 本地坐标 Y 轴移动
        /// </summary>
        public static Tweener LocalMoveY(
            Transform target,
            float endY,
            float duration = DefaultDuration,
            Ease ease = Ease.OutQuad,
            bool replaceOld = true,
            bool ignoreTimeScale = false,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target.DOLocalMoveY(endY, duration);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }

        #endregion
        

        #region UI 位置动画

        /// <summary>
        /// UI 锚点坐标移动
        /// </summary>
        public static Tweener AnchorMoveTo(
            RectTransform target,
            Vector2 endAnchoredPosition,
            float duration = DefaultDuration,
            Ease ease = Ease.OutQuad,
            bool replaceOld = true,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target.DOAnchorPos(endAnchoredPosition, duration);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }
        
        
        /// <summary>
        /// UI 锚点 X 轴移动
        /// </summary>
        public static Tweener AnchorMoveX(
            RectTransform target,
            float endX,
            float duration = DefaultDuration,
            Ease ease = Ease.OutQuad,
            bool replaceOld = true,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target.DOAnchorPosX(endX, duration);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }
        
        
        /// <summary>
        /// UI 锚点 Y 轴移动
        /// </summary>
        public static Tweener AnchorMoveY(
            RectTransform target,
            float endY,
            float duration = DefaultDuration,
            Ease ease = Ease.OutQuad,
            bool replaceOld = true,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target.DOAnchorPosY(endY, duration);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }
        
        
        /// <summary>
        /// UI 尺寸变化
        /// </summary>
        public static Tweener SizeTo(
            RectTransform target,
            Vector2 endSize,
            float duration = DefaultDuration,
            Ease ease = Ease.OutQuad,
            bool replaceOld = true,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target.DOSizeDelta(endSize, duration);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }

        #endregion
        

        #region 缩放动画

        /// <summary>
        /// 缩放到指定大小
        /// </summary>
        public static Tweener ScaleTo(
            Transform target,
            Vector3 endScale,
            float duration = DefaultDuration,
            Ease ease = Ease.OutBack,
            bool replaceOld = true,
            bool ignoreTimeScale = false,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target.DOScale(endScale, duration);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }
        
        
        /// <summary>
        /// 等比缩放到指定大小
        /// </summary>
        public static Tweener ScaleTo(
            Transform target,
            float endScale,
            float duration = DefaultDuration,
            Ease ease = Ease.OutBack,
            bool replaceOld = true,
            bool ignoreTimeScale = false,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target.DOScale(endScale, duration);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }
        
        
        /// <summary>
        /// 弹入显示
        /// </summary>
        public static Tweener PopIn(
            Transform target,
            Vector3 endScale,
            float duration = DefaultDuration,
            float startScale = 0f,
            Ease ease = Ease.OutBack,
            bool replaceOld = true,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            target.gameObject.SetActive(true);
            target.localScale = Vector3.one * startScale;

            Tweener tweener = target.DOScale(endScale, duration);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }
        
        
        /// <summary>
        /// 弹入显示，目标缩放默认为 Vector3.one
        /// </summary>
        public static Tweener PopIn(
            Transform target,
            float duration = DefaultDuration,
            float startScale = 0f,
            Ease ease = Ease.OutBack,
            bool replaceOld = true,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            return PopIn(
                target,
                Vector3.one,
                duration,
                startScale,
                ease,
                replaceOld,
                ignoreTimeScale,
                onComplete
            );
        }
        
        
        /// <summary>
        /// 弹出隐藏
        /// </summary>
        public static Tweener PopOut(
            Transform target,
            float duration = 0.2f,
            Ease ease = Ease.InBack,
            bool setInactiveOnComplete = true,
            bool replaceOld = true,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target.DOScale(Vector3.zero, duration);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                () =>
                {
                    if (setInactiveOnComplete && target != null)
                    {
                        target.gameObject.SetActive(false);
                    }

                    onComplete?.Invoke();
                }
            );

            return tweener;
        }
        
        
        /// <summary>
        /// Punch 缩放
        /// </summary>
        public static Tweener PunchScale(
            Transform target,
            Vector3 punch,
            float duration = 0.25f,
            int vibrato = 8,
            float elasticity = 0.8f,
            bool replaceOld = true,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target.DOPunchScale(
                punch,
                duration,
                vibrato,
                elasticity
            );

            ApplyCommonSettings(
                tweener,
                target,
                Ease.Unset,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }
        
        
        /// <summary>
        /// 默认 Punch 缩放
        /// </summary>
        public static Tweener PunchScale(
            Transform target,
            float duration = 0.25f,
            bool replaceOld = true,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            return PunchScale(
                target,
                DefaultPunchScale,
                duration,
                8,
                0.8f,
                replaceOld,
                ignoreTimeScale,
                onComplete
            );
        }
        
        
        /// <summary>
        /// 循环呼吸缩放
        /// </summary>
        public static Tweener LoopBreathScale(
            Transform target,
            float scaleMultiplier = 1.08f,
            float singleDuration = 0.6f,
            Ease ease = Ease.InOutSine,
            bool replaceOld = true,
            bool ignoreTimeScale = true)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Vector3 originScale = target.localScale;
            Vector3 endScale = originScale * scaleMultiplier;

            Tweener tweener = target
                .DOScale(endScale, singleDuration)
                .SetLoops(-1, LoopType.Yoyo);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                null
            );

            return tweener;
        }

        #endregion
        

        #region 旋转动画

        /// <summary>
        /// 旋转到指定欧拉角
        /// </summary>
        public static Tweener RotateTo(
            Transform target,
            Vector3 endEulerAngles,
            float duration = DefaultDuration,
            Ease ease = Ease.OutQuad,
            RotateMode rotateMode = RotateMode.Fast,
            bool replaceOld = true,
            bool ignoreTimeScale = false,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target.DORotate(
                endEulerAngles,
                duration,
                rotateMode
            );

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }


        /// <summary>
        /// 只旋转 Z 轴
        /// </summary>
        public static Tweener RotateZTo(
            Transform target,
            float endZ,
            float duration = DefaultDuration,
            Ease ease = Ease.OutQuad,
            RotateMode rotateMode = RotateMode.Fast,
            bool replaceOld = true,
            bool ignoreTimeScale = false,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            Vector3 endEuler = target.localEulerAngles;
            endEuler.z = endZ;

            return RotateTo(
                target,
                endEuler,
                duration,
                ease,
                rotateMode,
                replaceOld,
                ignoreTimeScale,
                onComplete
            );
        }


        /// <summary>
        /// Z 轴无限旋转
        /// </summary>
        public static Tweener LoopRotateZ(
            Transform target,
            float anglePerLoop = -360f,
            float singleDuration = 1f,
            bool replaceOld = true,
            bool ignoreTimeScale = true)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target
                .DOLocalRotate(
                    new Vector3(0f, 0f, anglePerLoop),
                    singleDuration,
                    RotateMode.FastBeyond360
                )
                .SetRelative()
                .SetLoops(-1, LoopType.Restart);

            ApplyCommonSettings(
                tweener,
                target,
                Ease.Linear,
                ignoreTimeScale,
                true,
                null
            );

            return tweener;
        }


        /// <summary>
        /// Z 轴来回摇摆
        /// </summary>
        public static Tweener LoopSwingZ(
            Transform target,
            float angle = 10f,
            float singleDuration = 0.45f,
            Ease ease = Ease.InOutSine,
            bool replaceOld = true,
            bool ignoreTimeScale = true)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Vector3 originEuler = target.localEulerAngles;
            Vector3 targetEuler = originEuler;
            targetEuler.z += angle;

            Tweener tweener = target
                .DOLocalRotate(targetEuler, singleDuration, RotateMode.Fast)
                .SetLoops(-1, LoopType.Yoyo);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                null
            );

            return tweener;
        }

        #endregion
        
        
        #region 震动动画

        /// <summary>
        /// 全局位置震动
        /// </summary>
        public static Tweener ShakePosition(
            Transform target,
            float duration = 0.25f,
            float strength = 0.2f,
            int vibrato = 15,
            float randomness = 90f,
            bool snapping = false,
            bool fadeOut = true,
            bool replaceOld = true,
            bool ignoreTimeScale = false,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);
            
            Tweener tweener = target.DOShakePosition(
                duration,
                strength,
                vibrato,
                randomness,
                snapping,
                fadeOut
            );
            
            ApplyCommonSettings(
                tweener,
                target,
                Ease.Unset,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }


        /// <summary>
        /// 本地位置震动
        /// </summary>
        public static Tweener ShakeLocalPosition(
            Transform target,
            float duration = 0.25f,
            float strength = 0.2f,
            int vibrato = 15,
            float randomness = 90f,
            bool snapping = false,
            bool fadeOut = true,
            bool replaceOld = true,
            bool ignoreTimeScale = false,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);
            
            Tweener tweener = target.DOShakePosition(
                duration,
                strength,
                vibrato,
                randomness,
                snapping,
                fadeOut
            );
            
            tweener.SetRelative();

            ApplyCommonSettings(
                tweener,
                target,
                Ease.Unset,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }


        /// <summary>
        /// 缩放震动
        /// </summary>
        public static Tweener ShakeScale(
            Transform target,
            float duration = 0.25f,
            float strength = 0.15f,
            int vibrato = 10,
            float randomness = 90f,
            bool fadeOut = true,
            bool replaceOld = true,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);
            
            Tweener tweener = target.DOShakeScale(
                duration,
                strength,
                vibrato,
                randomness,
                fadeOut
            );

            ApplyCommonSettings(
                tweener,
                target,
                Ease.Unset,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }

        #endregion
        
        
        #region CanvasGroup 淡入淡出

        /// <summary>
        /// CanvasGroup 淡入 / 淡出到指定透明度
        /// </summary>
        public static Tweener FadeCanvasGroup(
            CanvasGroup target,
            float endAlpha,
            float duration = DefaultDuration,
            Ease ease = Ease.OutQuad,
            bool replaceOld = true,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            Tweener tweener = target.DOFade(endAlpha, duration);

            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                onComplete
            );

            return tweener;
        }


        /// <summary>
        /// 显示 CanvasGroup
        /// </summary>
        public static Tweener ShowCanvasGroup(
            CanvasGroup target,
            float duration = DefaultDuration,
            bool fromZero = true,
            bool interactableOnComplete = true,
            bool blocksRaycastsOnComplete = true,
            Ease ease = Ease.OutQuad,
            bool replaceOld = true,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            target.gameObject.SetActive(true);
            
            if (fromZero)
                target.alpha = 0f;
            
            target.interactable = false;
            target.blocksRaycasts = false;
            
            Tweener tweener = target.DOFade(1f, duration);
            
            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                () =>
                {
                    if (target != null)
                    {
                        target.interactable = interactableOnComplete;
                        target.blocksRaycasts = blocksRaycastsOnComplete;
                    }

                    onComplete?.Invoke();
                }
            );

            return tweener;
        }


        /// <summary>
        /// 隐藏 CanvasGroup
        /// </summary>
        public static Tweener HideCanvasGroup(
            CanvasGroup target,
            float duration = DefaultDuration,
            bool setInactiveOnComplete = true,
            Ease ease = Ease.OutQuad,
            bool replaceOld = true,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);

            target.interactable = false;
            target.blocksRaycasts = false;
            
            Tweener tweener = target.DOFade(0f, duration);
            
            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                () =>
                {
                    if (setInactiveOnComplete && target != null)
                    {
                        target.gameObject.SetActive(false);
                    }

                    onComplete?.Invoke();
                }
            );

            return tweener;
        }

        #endregion
        
        
        #region UI 组合动画
        
        /// <summary>
        /// UI 弹窗显示动画
        ///
        /// 组合内容
        /// 1. CanvasGroup 淡入
        /// 2. RectTransform 缩放弹入
        /// </summary>
        public static Sequence ShowPopup(
            CanvasGroup canvasGroup,
            RectTransform root,
            float duration = 0.25f,
            float startScale = 0.85f,
            float endScale = 1f,
            bool replaceOld = true,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            if (canvasGroup == null || root == null)
                return null;

            if (replaceOld)
            {
                DOTween.Kill(canvasGroup);
                DOTween.Kill(root);
            }
            
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            root.localScale = Vector3.one * startScale;
            
            Sequence sequence = DOTween.Sequence();

            sequence.Join(canvasGroup.DOFade(1f, duration * 0.8f));
            sequence.Join(root.DOScale(endScale, duration).SetEase(Ease.OutBack));
            
            ApplyCommonSettings(
                sequence,
                root,
                Ease.Unset,
                ignoreTimeScale,
                true,
                () =>
                {
                    if (canvasGroup != null)
                    {
                        canvasGroup.interactable = true;
                        canvasGroup.blocksRaycasts = true;
                    }

                    onComplete?.Invoke();
                }
            );

            return sequence;
        }



        /// <summary>
        /// UI 弹窗隐藏动画
        ///
        /// 组合内容：
        /// 1. CanvasGroup 淡出
        /// 2. RectTransform 缩小
        /// </summary>
        public static Sequence HidePopup(
            CanvasGroup canvasGroup,
            RectTransform root,
            float duration = 0.2f,
            float endScale = 0.85f,
            bool setInactiveOnComplete = true,
            bool replaceOld = true,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            if (canvasGroup == null || root == null)
                return null;
            
            if (replaceOld)
            {
                DOTween.Kill(canvasGroup);
                DOTween.Kill(root);
            }
            
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            Sequence sequence = DOTween.Sequence();
            
            sequence.Join(canvasGroup.DOFade(0f, duration));
            sequence.Join(root.DOScale(endScale, duration).SetEase(Ease.InBack));
            
            ApplyCommonSettings(
                sequence,
                root,
                Ease.Unset,
                ignoreTimeScale,
                true,
                () =>
                {
                    if (setInactiveOnComplete && canvasGroup != null)
                    {
                        canvasGroup.gameObject.SetActive(false);
                    }

                    onComplete?.Invoke();
                }
            );

            return sequence;
        }


        /// <summary>
        /// UI 上下浮动循环
        /// </summary>
        public static Tweener LoopUIFloatY(
            RectTransform target,
            float offsetY = 10f,
            float singleDuration = 0.8f,
            Ease ease = Ease.InOutSine,
            bool replaceOld = true,
            bool ignoreTimeScale = true)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);
            
            Vector2 originPos = target.anchoredPosition;
            Vector2 endPos = originPos + new Vector2(0f, offsetY);
            
            Tweener tweener = target.DOAnchorPos(endPos, singleDuration)
                                    .SetLoops(-1, LoopType.Yoyo);
            
            ApplyCommonSettings(
                tweener,
                target,
                ease,
                ignoreTimeScale,
                true,
                null
            );

            return tweener;
        }


        /// <summary>
        /// UI 左右摇动
        /// </summary>
        public static Tweener UIShakeAnchorX(
            RectTransform target,
            float strength = 20f,
            float duration = 0.25f,
            int vibrato = 20,
            bool replaceOld = true,
            bool ignoreTimeScale = true,
            Action onComplete = null)
        {
            if (target == null)
                return null;

            if (replaceOld)
                DOTween.Kill(target);
            
            Vector2 originPos = target.anchoredPosition;

            Tweener tweener = target.DOShakeAnchorPos(
                duration,
                new Vector2(strength, 0f),
                vibrato,
                90f,
                false,
                true
            );
            
            ApplyCommonSettings(
                tweener,
                target,
                Ease.Unset,
                ignoreTimeScale,
                true,
                () =>
                {
                    if (target != null)
                    {
                        target.anchoredPosition = originPos;
                    }

                    onComplete?.Invoke();
                }
            );

            return tweener;
        }

        #endregion
        

        #region 辅助方法

        /// <summary>
        /// 应用通用 Tween 设置
        /// </summary>
        /// <param name="tween"></param>
        /// <param name="target"></param>
        /// <param name="ease"></param>
        /// <param name="ignoreTimeScale"></param>
        /// <param name="autoKill"></param>
        /// <param name="onComplete"></param>
        /// <typeparam name="T"></typeparam>
        private static void ApplyCommonSettings<T>(
            T tween, 
            UnityEngine.Object target, 
            Ease ease, 
            bool ignoreTimeScale,
            bool autoKill, 
            Action onComplete) where T : Tween
        {
            if (tween == null) return;

            if (target != null)
            {
                tween.SetTarget(target);
                
                GameObject linkGo = ResolveGameObject(target);
                if (linkGo != null)
                    tween.SetLink(linkGo, LinkBehaviour.KillOnDestroy);
            }

            if (ease != Ease.Unset)
                tween.SetEase(ease);
            
            tween.SetUpdate(ignoreTimeScale);
            tween.SetAutoKill(autoKill);
            
            if (onComplete != null)
                tween.OnComplete(onComplete.Invoke);
        }


        /// <summary>
        /// 从 UnityEngine.Object 中解析 GameObject
        /// 用于 SetLink，使目标对象销毁时 Tween 自动 Kill
        /// </summary>
        private static GameObject ResolveGameObject(UnityEngine.Object target)
        {
            if (target == null)
                return null;

            if (target is GameObject go)
                return go;
            
            if (target is Component component)
                return component.gameObject;
            
            return null;
        }

        #endregion
    }
}
