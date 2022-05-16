using UnityEngine;

namespace Ragnarok
{
    public class UICostumePreview : UICanvas
    {
        public class Input : IUIData
        {
            public ItemInfo costume;

            public Input(ItemInfo costume)
            {
                this.costume = costume;
            }
        }

        [SerializeField] UILabelHelper titleLabel;
        [SerializeField] UIUnitViewer unitViewer;
        [SerializeField] UIButtonHelper okButton;
        [SerializeField] UIButtonHelper closeButton;

        protected override UIType uiType => UIType.Back | UIType.Destroy;

        private CostumePreviewPresenter presenter;

        protected override void OnInit()
        {
            presenter = new CostumePreviewPresenter();
            presenter.AddEvent();
            EventDelegate.Add(okButton.OnClick, OnClickClose);
            EventDelegate.Add(closeButton.OnClick, OnClickClose);
        }

        protected override void OnClose()
        {
            presenter.Dispose();
            presenter.RemoveEvent();
            EventDelegate.Remove(okButton.OnClick, OnClickClose);
            EventDelegate.Remove(closeButton.OnClick, OnClickClose);
        }

        protected override void OnShow(IUIData data = null)
        {
            unitViewer.Show(presenter.GetDummyUIPlayer((data as Input).costume));
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            titleLabel.Text = LocalizeKey._40226.ToText(); // 코스튬 미리보기
            okButton.Text = LocalizeKey._2902.ToText();
        }

        private void OnClickClose()
        {
            UI.Close<UICostumePreview>();
        }
    }
}
