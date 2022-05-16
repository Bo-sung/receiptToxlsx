﻿using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIPackageCatCoin : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButton background;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelRewardDesc;
        [SerializeField] UILabelHelper labelRibbonTitle;
        [SerializeField] UILabelHelper labelRewardTitle;
        [SerializeField] UILabelHelper labelRoPoint;
        [SerializeField] UILabelHelper labelSecretTicket;
        [SerializeField] UILabelHelper labelShareTicket;
        [SerializeField] UILabelHelper labelCatCoin;
        [SerializeField] UILabelHelper labelZeny;
        [SerializeField] UILabelHelper labelBonus;
        [SerializeField] UILabelHelper labelEveryDay;
        [SerializeField] UILabelHelper labelCoolingOff;
        [SerializeField] UILabelHelper labelReaminCount;
        [SerializeField] EveryDayRewardSlot[] everyDayRewardSlots;
        [SerializeField] UIButtonhWithGrayScale btnPurchase;
        [SerializeField] UILabelHelper labelNotice;

        PackageCatCoinPresenter presenter;

        protected override void OnInit()
        {
            presenter = new PackageCatCoinPresenter();

            presenter.OnPucharseSuccess += Refresh;
            presenter.OnUpdateEveryDayGoods += Refresh;

            presenter.AddEvent();

            EventDelegate.Add(background.onClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnPurchase.OnClick, presenter.OnClickedBtnPurchase);
        }

        protected override void OnClose()
        {
            presenter.OnPucharseSuccess -= Refresh;
            presenter.OnUpdateEveryDayGoods -= Refresh;

            presenter.RemoveEvent();

            EventDelegate.Remove(background.onClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnPurchase.OnClick, presenter.OnClickedBtnPurchase);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._4400; // 냥다래 프리미엄
            labelRewardDesc.LocalKey = LocalizeKey._4401; // 총 1,340 냥다래!! 301%의 가치!!
            labelRibbonTitle.LocalKey = LocalizeKey._4402; // 기간한정
            labelRewardTitle.LocalKey = LocalizeKey._4403; // 구매 즉시 지급
            labelRoPoint.LocalKey = LocalizeKey._4404; // RO POINT\n20
            labelSecretTicket.LocalKey = LocalizeKey._4405; // 비밀 상점 갱신권\n2개
            labelShareTicket.LocalKey = LocalizeKey._4406; // 시계 16H\n1개
            labelCatCoin.LocalKey = LocalizeKey._4407; // 냥다래 200개
            labelZeny.LocalKey = LocalizeKey._4408; // 100,000 제니
            labelBonus.LocalKey = LocalizeKey._4409; // 28일간 한정 특권 발동
            labelEveryDay.LocalKey = LocalizeKey._4410; // 매일 매일\n수령가능
            labelNotice.LocalKey = LocalizeKey._90296; // 해당 상품은 우편함에서 수령 시 바로 한정 특권이 적용되고,\n보상을 수령하지 않으면 한정 특권의 횟수가 차감되지 않습니다.

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
            UI.Close<UIPackageCatCoin>();
        }

        public void Set(int shopId)
        {
            presenter.SetShopId(shopId);
        }

        void Refresh()
        {
            presenter.GetRemainCount();

            EveryDayPackageState state = presenter.GetEveryDayPackageState();

            switch (state)
            {
                case EveryDayPackageState.CompletePurchase:
                    // 구매 완료 (구매 후 우편함에서 미수령)
                    btnPurchase.IsEnabled = false;
                    btnPurchase.SetMode(UIGraySprite.SpriteMode.Grayscale);
                    btnPurchase.LocalKey = LocalizeKey._4412; // 구매 완료
                    labelReaminCount.SetActive(false);
                    break;

                case EveryDayPackageState.AvailablePurchase:
                    // 구매 가능 (첫구매나, 재구매)
                    btnPurchase.IsEnabled = true;
                    btnPurchase.SetMode(UIGraySprite.SpriteMode.None);
                    btnPurchase.Text = presenter.GetPrice();
                    labelReaminCount.SetActive(false);
                    break;

                case EveryDayPackageState.StandByReward:
                    // 받을 수 있는 보상이 있음
                    btnPurchase.IsEnabled = true;
                    btnPurchase.SetMode(UIGraySprite.SpriteMode.None);
                    btnPurchase.LocalKey = LocalizeKey._4413; // 수령 받기
                    SetRemainCount();
                    break;

                case EveryDayPackageState.ReceivedReward:
                    // 금일 보상을 이미 받은 상태
                    btnPurchase.IsEnabled = true;
                    btnPurchase.SetMode(UIGraySprite.SpriteMode.Grayscale);
                    btnPurchase.LocalKey = LocalizeKey._4414; // 수령 완료
                    SetRemainCount();
                    break;
            }

            btnPurchase.SetNotice(state == EveryDayPackageState.StandByReward);
            SetEveryDayRewards();
        }

        private void SetRemainCount()
        {
            labelReaminCount.SetActive(true);
            labelReaminCount.Text = LocalizeKey._4411.ToText().Replace(ReplaceKey.VALUE, presenter.GetRemainCount()); // 잔여 횟수 : [c][587DE1]{VALUE}[-][/c]
        }

        private void SetEveryDayRewards()
        {
            RewardGroupData[] rewards = presenter.GetEveryDayRewards();
            for (int i = 0; i < everyDayRewardSlots.Length; i++)
            {
                if (i < rewards.Length)
                {
                    RewardData reward = rewards[i].GetReward();
                    everyDayRewardSlots[i].Set(reward.IconName, reward.Count);
                }
            }
        }

        public override bool Find()
        {
            base.Find();

            everyDayRewardSlots = GetComponentsInChildren<EveryDayRewardSlot>();
            return true;
        }
    }
}