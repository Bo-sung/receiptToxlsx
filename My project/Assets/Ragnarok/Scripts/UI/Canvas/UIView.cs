using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public abstract class UIView : MonoBehaviour, IAutoInspectorFinder
    {
        GameObject myGameObject;
        public bool IsShow { get; private set; }

        protected virtual void Awake()
        {
            myGameObject = gameObject;
            UI.AddEventLocalize(OnLocalize);
        }

        protected virtual void Start()
        {
            IsShow = true;
            OnLocalize();
        }

        protected virtual void OnDestroy()
        {
            UI.RemoveEventLocalize(OnLocalize);
            myGameObject = null;
        }

        protected abstract void OnLocalize();

        public virtual void Show()
        {
            SetActive(true);
        }

        public virtual void Hide()
        {
            SetActive(false);
        }

        public void SetActive(bool isActive)
        {
            IsShow = isActive;
            NGUITools.SetActive(myGameObject, isActive);
        }
    }

    [System.Obsolete("UIView로 변경 예정")]
    public abstract class UIView<T> : UIView
    {
        protected List<T> listeners = new List<T>();

        public void AddListener(T listener)
        {
            listeners.Add(listener);
        }

        public void RemoveListener(T listener)
        {
            listeners.Remove(listener);
        }
    }
}