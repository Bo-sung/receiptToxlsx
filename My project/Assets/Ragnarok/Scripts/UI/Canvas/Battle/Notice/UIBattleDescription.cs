using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleDescription : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UITextureHelper itemIcon;
        [SerializeField] UITweener playTween;

        BattleDescriptionPresenter presenter;

        private int localKey;

        protected override void OnInit()
        {
            presenter = new BattleDescriptionPresenter();
            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            Refresh();
        }

        public void Initialize(int localKey)
        {
            this.localKey = localKey;

            // Set Icon
            itemIcon.SetActive(false);
            playTween.Play(forward: false);

            Refresh();
        }

        public void Initialize(int localKey, int itemId)
        {
            this.localKey = localKey;

            // Set Icon
            itemIcon.SetActive(true);
            itemIcon.SetItem(presenter.GetItemIconName(itemId));
            playTween.Play(forward: true);

            Refresh();
        }

        private void Refresh()
        {
            if (localKey == 0)
            {
                labelDescription.SetActive(false);
                return;
            }

            labelDescription.SetActive(true);
            labelDescription.LocalKey = localKey;
        }
    }
}