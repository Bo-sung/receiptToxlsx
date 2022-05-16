using UnityEngine;

namespace Ragnarok
{
    public class UIDanger : UICanvas<DangerPresenter>
        , DangerPresenter.IView
    {
        [SerializeField] GameObject danger;
        [SerializeField] GameObject share;

        GameObject cachedGameObject;

        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override bool IsVisible => true;

        protected override void OnInit()
        {
            cachedGameObject = gameObject;

            presenter = new DangerPresenter(this);
            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            if (presenter != null)
                presenter = null;
        }

        protected override void OnShow(IUIData data = null)
        {
            Hide();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void ChangeCharacterControl(bool isPlayer)
        {
            presenter.ChangeCharacterControl(isPlayer);
        }

        void DangerPresenter.IView.SetActiveDanger(bool isActive)
        {
            cachedGameObject.SetActive(isActive);
            if (isActive)
            {
                danger.SetActive(true);
                share.SetActive(false);
            }
        }

        void DangerPresenter.IView.SetActiveShare(bool isActive)
        {
            cachedGameObject.SetActive(isActive);
            if (isActive)
            {
                danger.SetActive(false);
                share.SetActive(true);
            }
        }
    }
}