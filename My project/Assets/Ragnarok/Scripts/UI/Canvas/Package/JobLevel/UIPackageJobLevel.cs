using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIPackageJobLevel : UICanvas
    {
        protected override UIType uiType => UIType.Hide | UIType.Back;

        [SerializeField] UIButton background;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelRewardDesc;
        [SerializeField] UILabelHelper labelRemainTime;
        [SerializeField] UIGrid gridReward;
        [SerializeField] UIRewardHelper[] rewards;
        [SerializeField] UILabelHelper labelCoolingOff;
        [SerializeField] UIButtonhWithGrayScale btnPurchase;
        [SerializeField] UILabelHelper labelPoringText;
        [SerializeField] GameObject bonus, sale;
        [SerializeField] UILabelHelper labelBonus, labelSale;
        [SerializeField] int maxColumn = 4;
        [SerializeField] int maxWidth = 540;

        PackageJobLevelPresenter presenter;

        protected override void OnInit()
        {
            presenter = new PackageJobLevelPresenter();
            presenter.OnPucharseSuccess += CloseUI;

            presenter.AddEvent();

            EventDelegate.Add(background.onClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnPurchase.OnClick, presenter.OnClickedBtnPurchase);
        }

        protected override void OnClose()
        {
            presenter.OnPucharseSuccess -= CloseUI;

            presenter.RemoveEvent();

            EventDelegate.Remove(background.onClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnPurchase.OnClick, presenter.OnClickedBtnPurchase);
        }

        protected override void OnShow(IUIData data = null)
        {
            if (!presenter.Set())
            {
                CloseUI();
                return;
            }
            Refresh();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            if (GameServerConfig.IsKoreaLanguage())
            {
                const int LOCAL_KEY = LocalizeKey._90302; // 구매하는 상품의 수량 및 금액을 반드시 확인하시기 바랍니다.\n유료재화로 구매된 상품은 구매 후 7일 이내에는 청약철회가 가능합니다.\n상품을 사용하였거나 즉시 적용되는 상품의 경우, 추가 혜택이 사용된 경우 청약 철회 대상에서 제외됩니다.\n(유료 재화부터 우선 소모 됩니다.)
                labelCoolingOff.Text = BasisUrl.KoreanPurchaseTerms.AppendText(LOCAL_KEY.ToText());
            }
            else
            {
                labelCoolingOff.LocalKey = LocalizeKey._90220; // 구매한 아이템은 우편함으로 지급 됩니다.\n우편함에서 해당 상품의 구성품을 수령 할 경우, 청약 철회가 불가능합니다.
            }
        }

        void CloseUI()
        {
            UI.Close<UIPackageJobLevel>();
        }

        void Refresh()
        {
            labelTitle.Text = presenter.GetTitle();
            labelRewardDesc.Text = presenter.GetDescription();
            labelPoringText.Text = presenter.GetPoringText();
            SetReward();
            SetPrice();
            SetProductType();
            Timing.RunCoroutineSingleton(YieldRemainTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private void SetReward()
        {
            RewardGroupData[] data = presenter.GetRewards();

            for (int i = 0; i < rewards.Length; i++)
            {
                if (i < data.Length)
                {
                    rewards[i].SetData(data[i].GetReward());
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

        private void SetPrice()
        {
            bool canBuy = presenter.CanBuy();

            if (canBuy)
            {
                btnPurchase.IsEnabled = true;
                btnPurchase.SetMode(UIGraySprite.SpriteMode.None);
                btnPurchase.Text = presenter.GetPrice();
            }
            else
            {
                btnPurchase.IsEnabled = false;
                btnPurchase.SetMode(UIGraySprite.SpriteMode.Grayscale);
                btnPurchase.LocalKey = LocalizeKey._4701; // 구매 불가
            }
        }

        private void SetProductType()
        {
            ShopProductType type = presenter.GetProductValueType();
            int value = presenter.GetProductValue();

            switch (type)
            {
                case ShopProductType.None:
                    bonus.SetActive(false);
                    sale.SetActive(false);
                    break;

                case ShopProductType.Bonus:
                    bonus.SetActive(true);
                    sale.SetActive(false);
                    labelBonus.Text = LocalizeKey._4702.ToText().Replace(ReplaceKey.VALUE, value); // {VALUE}배 가치!
                    break;

                case ShopProductType.Sale:
                    bonus.SetActive(false);
                    sale.SetActive(true);
                    labelSale.Text = LocalizeKey._4703.ToText().Replace(ReplaceKey.VALUE, value); // {VALUE}% 할인!
                    break;
            }
        }

        /// <summary>
        /// 남은시간 체크
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> YieldRemainTime()
        {
            while (true)
            {
                float time = presenter.GetRemainTime();
                if (time <= 0)
                    break;

                UpdateLimitTime(time);
                yield return Timing.WaitForSeconds(0.5f);
            }
            UpdateLimitTime(0);
            SetPrice();
        }

        private void UpdateLimitTime(float time)
        {
            if (time <= 0)
            {
                labelRemainTime.Text = LocalizeKey._4700.ToText().Replace(ReplaceKey.TIME, "00:00"); ; // 남은 시간 : {TIME}
                return;
            }

            // UI 표시에 1분을 추가해서 보여준다.
            TimeSpan span = TimeSpan.FromMilliseconds(time + 60000);
            int totalDays = (int)span.TotalDays;
            bool isDay = totalDays > 0;

            if (isDay)
            {
                string dayTime = LocalizeKey._8041.ToText().Replace(ReplaceKey.TIME, totalDays); // D-{TIME}
                labelRemainTime.Text = LocalizeKey._4700.ToText().Replace(ReplaceKey.TIME, dayTime); // 남은 시간 : {TIME}
            }
            else
            {
                labelRemainTime.Text = LocalizeKey._4700.ToText().Replace(ReplaceKey.TIME, span.ToString(@"hh\:mm")); // 남은 시간 : {TIME}
            }
        }

        public override bool Find()
        {
            base.Find();
            rewards = GetComponentsInChildren<UIRewardHelper>();
            return true;
        }
    }
}