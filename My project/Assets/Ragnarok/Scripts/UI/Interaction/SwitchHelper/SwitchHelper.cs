using UnityEngine;

namespace Ragnarok
{
    public abstract class SwitchHelper<T> : MonoBehaviour
    {
        [SerializeField] protected T on;
        [SerializeField] protected T off;

        private bool isInitialize;
        private bool isOn;

        protected GameObject myGameObject;

        protected virtual void Awake()
        {
            myGameObject = gameObject;
        }

        public void Switch(bool isOn)
        {
            if (isInitialize && this.isOn == isOn)
                return;

            isInitialize = true;
            this.isOn = isOn;

            Execute(isOn ? on : off);
        }

        protected abstract void Execute(T value);

        protected virtual void Reset()
        {
        }

        public void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }

        public void Show()
        {
            SetActive(true);
        }

        public void Hide()
        {
            SetActive(false);
        }
    }
}