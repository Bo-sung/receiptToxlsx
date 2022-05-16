using UnityEngine;

namespace Ragnarok
{
    public class UIGoodsInfo : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButtonHelper background;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UITextureHelper goodsIcon;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] GameObject owned;
        [SerializeField] UILabelHelper labelOwned;
        [SerializeField] UILabelHelper labelOwnedValue;

        GoodsInfoPresenter presenter;

        protected override void OnInit()
        {
            presenter = new GoodsInfoPresenter();
            presenter.AddEvent();
            EventDelegate.Add(background.OnClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(background.OnClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
        }

        protected override void OnShow(IUIData data)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._46200; // 재화 정보
            labelOwned.LocalKey = LocalizeKey._46201; // 보유
        }

        public void SetData(RewardType rewardType)
        {
            goodsIcon.Set(rewardType.IconName());
            labelName.Text = rewardType.GetItemName();
            labelDescription.Text = rewardType.GetDesc();

            if (presenter.IsShowOwned(rewardType))
            {
                owned.SetActive(true);
                labelOwnedValue.Text = presenter.GetOwnedValue(rewardType).ToString("N0");
            }
            else
            {
                owned.SetActive(false);
            }
        }

        private void CloseUI()
        {
            UI.Close<UIGoodsInfo>();
        }
    }
}
