using UnityEngine;

namespace Ragnarok
{
    public class UIToggleWithLock : UIToggleHelper
    {
        [SerializeField] int index;
        [SerializeField] GameObject goLock;

        public event System.Action<int> OnSelectLock;

        protected void Awake()
        {
            if (goLock)
            {
                UIEventListener.Get(goLock).onClick += OnClockedLock;
            }
        }

        protected void OnDestroy()
        {
            if (goLock)
            {
                UIEventListener.Get(goLock).onClick -= OnClockedLock;
            }
        }

        public void SetActiveLock(bool isActive)
        {
            NGUITools.SetActive(goLock, isActive);
        }

        void OnClockedLock(GameObject go)
        {
            OnSelectLock?.Invoke(index);
        }

        public override bool Find()
        {
            base.Find();

            index = transform.GetSiblingIndex();
            return true;
        }
    }
}