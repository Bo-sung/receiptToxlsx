using UnityEngine;

namespace Ragnarok
{
    [System.Obsolete("UIView로 변경 예정")]
    public abstract class UIInfo<TInfo> : MonoBehaviour, IAutoInspectorFinder
        where TInfo : class, IInfo
    {
        GameObject myGameObject;

        protected TInfo info;

        protected virtual void Awake()
        {
            myGameObject = gameObject;
        }

        protected virtual void Start()
        {
            OnLocalize();
        }

        protected virtual void OnDestroy()
        {
            myGameObject = null;

            RemoveEvent();

            if (info != null)
                info = null;
        }

        protected virtual void OnDisable()
        {
        }

        public virtual void SetData(TInfo info)
        {
            RemoveEvent();

            this.info = info;

            AddEvent();
            Refresh();
        }

        protected virtual void OnLocalize()
        {
            Refresh();
        }

        /// <summary>
        /// 무효한 데이터
        /// </summary>
        protected bool IsInvalid()
        {
            return info == null || info.IsInvalidData;
        }

        private void AddEvent()
        {
            if (info == null)
                return;

            AddInfoEvent();
        }

        private void RemoveEvent()
        {
            if (info == null)
                return;

            RemoveInfoEvent();
        }

        protected abstract void Refresh();

        protected virtual void AddInfoEvent()
        {
            info.OnUpdateEvent += Refresh;
        }

        protected virtual void RemoveInfoEvent()
        {
            info.OnUpdateEvent -= Refresh;
        }

        public virtual void SetActive(bool isActive)
        {
            myGameObject.SetActive(isActive);
        }
    }

    public abstract class UIInfo<TPresenter, TInfo> : UIInfo<TInfo>
        where TPresenter : ViewPresenter
        where TInfo : class, IInfo
    {
        protected TPresenter presenter;

        public virtual void Initialize(TPresenter presenter)
        {
            this.presenter = presenter;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (presenter != null)
                presenter = null;
        }

        public virtual void SetData(TPresenter presenter, TInfo info)
        {
            Initialize(presenter);
            SetData(info);
        }
    }
}