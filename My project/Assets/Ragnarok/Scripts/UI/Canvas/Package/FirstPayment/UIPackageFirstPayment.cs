using UnityEngine;

namespace Ragnarok
{
    public sealed class UIPackageFirstPayment : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UIButton background;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelRewardDesc;
        [SerializeField] UIGrid gridReward;
        [SerializeField] UIRewardHelper[] rewards;
        [SerializeField] UIButtonhWithGrayScale btnConfirm;
        [SerializeField] int maxColumn = 4;
        [SerializeField] int maxWidth = 540;

        PackageFirstPaymentPresenter presenter;

        protected override void OnInit()
        {
            presenter = new PackageFirstPaymentPresenter();
            presenter.AddEvent();

            EventDelegate.Add(background.onClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnConfirm.OnClick, ShortCutShop);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(background.onClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnConfirm.OnClick, ShortCutShop);
        }

        protected override void OnShow(IUIData data = null)
        {
            Refresh();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnConfirm.LocalKey = LocalizeKey._4900; // 확인
        }

        void CloseUI()
        {
            UI.Close<UIPackageFirstPayment>();
        }

        void ShortCutShop()
        {
            UI.ShortCut<UIShop>().Set(UIShop.ViewType.Default);
        }

        void Refresh()
        {
            labelTitle.Text = presenter.GetTitle();
            labelRewardDesc.Text = presenter.GetDescription();
            icon.SetShop(presenter.GetIconName());
            SetReward();
        }

        private void SetReward()
        {
            RewardData[] data = presenter.GetRewards();

            for (int i = 0; i < rewards.Length; i++)
            {
                if (i < data.Length)
                {
                    rewards[i].SetData(data[i]);
                }
                else
                {
                    rewards[i].SetData(null);
                }
            }

            int count = data.Length;
            if (count <= maxColumn)
            {
                gridReward.maxPerLine = 0;
                gridReward.cellWidth = maxWidth / count;
            }
            else
            {
                int columnLimt = MathUtils.RoundToInt(count * 0.5f);
                gridReward.maxPerLine = columnLimt;
                gridReward.cellWidth = maxWidth / columnLimt;
            }

            gridReward.Reposition();
        }

        public override bool Find()
        {
            base.Find();
            rewards = GetComponentsInChildren<UIRewardHelper>();
            return true;
        }
    }
}