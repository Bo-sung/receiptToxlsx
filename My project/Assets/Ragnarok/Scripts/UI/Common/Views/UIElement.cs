using UnityEngine;

namespace Ragnarok.View
{
    public abstract class UIElement<T> : MonoBehaviour, IAutoInspectorFinder
        where T : class
    {
        GameObject myGameObject;

        protected T info;

        protected virtual void Awake()
        {
            myGameObject = gameObject;
            UI.AddEventLocalize(OnLocalize);
        }

        protected virtual void Start()
        {
            OnLocalize();
        }

        protected virtual void OnDestroy()
        {
            UI.RemoveEventLocalize(OnLocalize);
            myGameObject = null;

            TryRemoveEvent();

            if (info != null)
                info = null;
        }

        public void SetData(T input)
        {
            TryRemoveEvent();

            info = input;

            if (info == null)
            {
                SetActive(false);
                return;
            }

            TryAddEvent();
            SetActive(true);
            Refresh();
        }

        protected abstract void OnLocalize();

        protected abstract void Refresh();

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }

        private void TryAddEvent()
        {
            if (info == null)
                return;

            AddEvent();
        }

        private void TryRemoveEvent()
        {
            if (info == null)
                return;

            RemoveEvent();
        }

        protected virtual void AddEvent()
        {
        }

        protected virtual void RemoveEvent()
        {
        }
    }
}