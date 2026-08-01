using UnityEngine;

namespace ClientFramework.UI
{
    public abstract class UIBase : MonoBehaviour
    {
        private bool created;
        private bool eventsBound;
        private bool visible;
        private bool released;

        internal void InternalCreate()
        {
            if (created || released)
                return;

            created = true;
            OnCreate();
        }

        internal void InternalShow()
        {
            if (released)
                return;

            InternalCreate();
            gameObject.SetActive(true);

            if (!eventsBound)
            {
                BindEvents();
                eventsBound = true;
            }

            visible = true;
            UIUpdate();

            if (!visible || released)
                return;

            OnShow();

            if (!visible || released)
                return;

            PlayShowAnimation();
        }

        internal void InternalHide()
        {
            if (!visible && !gameObject.activeSelf)
                return;

            visible = false;
            KillAnimations();
            OnHide();

            if (eventsBound)
            {
                UnbindEvents();
                eventsBound = false;
            }

            gameObject.SetActive(false);
        }

        internal void InternalRelease()
        {
            if (released)
                return;

            released = true;

            if (visible || gameObject.activeSelf)
                InternalHide();

            KillAnimations();

            if (eventsBound)
            {
                UnbindEvents();
                eventsBound = false;
            }

            OnRelease();
        }

        protected virtual void OnCreate()
        {
        }

        protected virtual void BindEvents()
        {
        }

        protected virtual void UnbindEvents()
        {
        }

        public virtual void UIUpdate()
        {
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }

        protected virtual void OnRelease()
        {
        }

        protected virtual void PlayShowAnimation()
        {
        }

        protected virtual void KillAnimations()
        {
        }
    }
}
