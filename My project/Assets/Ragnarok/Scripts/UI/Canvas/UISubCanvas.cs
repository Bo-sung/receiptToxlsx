using UnityEngine;

namespace Ragnarok
{
    [System.Obsolete("UIView 로 변경 예정")]
    public abstract class UISubCanvas : MonoBehaviour, IAutoInspectorFinder
    {
        public virtual void Init()
        {
            OnInit();
        }

        public virtual void Close()
        {
            OnHide();
            OnClose();
        }

        public virtual void Localize()
        {
            OnLocalize();
        }

        public virtual void Show()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            OnShow();
            OnLocalize();
        }

        public void Hide()
        {
            OnHide();
            gameObject.SetActive(false);
        }

        public void SetActive(bool isActive)
        {
            if (isActive)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public bool IsVisible => gameObject.activeSelf;

        protected abstract void OnInit();
        protected abstract void OnClose();
        protected abstract void OnShow();
        protected abstract void OnHide();
        protected abstract void OnLocalize();
    }

    [System.Obsolete("UIView 방식을 변경 예정")]
    public abstract class UISubCanvas<T> : UISubCanvas
        where T : ViewPresenter
    {
        protected T presenter;

        public virtual void Initialize(T presenter)
        {
            this.presenter = presenter;
        }

        public override void Close()
        {
            base.Close();

            presenter = null;
        }
    }

    [System.Obsolete("이벤트 방식으로 수정 예정")]
    public abstract class UISubCanvasListener<T> : UISubCanvas
        where T : class
    {
        protected T listener;

        public virtual void Initialize(T listener)
        {
            this.listener = listener;
        }

        public override void Close()
        {
            base.Close();

            listener = null;
        }
    }
}