using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIPackageOnBuffPass : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButton background;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelRewardDesc;
        [SerializeField] UILabelHelper labelCoolingOff;
        [SerializeField] UIButtonhWithGrayScale btnPurchase;
        [SerializeField] UILabelHelper labelRemainTime;
        [SerializeField] UILabelHelper labelRewardTitle;
        [SerializeField] UILabelHelper labelRewardValue1;
        [SerializeField] UILabelHelper labelRewardValue2;
        [SerializeField] UILabelHelper labelJobLevel;
        [SerializeField] UILabelHelper labelAccountConnectNotice;

        PackageBattlePassPresenter presenter;

        protected override void OnInit()
        {
            presenter = new PackageBattlePassPresenter(PassType.OnBuff);
            presenter.OnPucharseSuccess += Refresh;

            presenter.AddEvent();

            EventDelegate.Add(background.onClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnPurchase.OnClick, presenter.OnClickedBtnPurchase);
        }

        protected override void OnClose()
        {
            presenter.OnPucharseSuccess -= Refresh;

            presenter.RemoveEvent();

            EventDelegate.Remove(background.onClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnPurchase.OnClick, presenter.OnClickedBtnPurchase);
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
            if (GameServerConfig.IsKoreaLanguage())
            {
                const int LOCAL_KEY = LocalizeKey._90302; // 구매하는 상품의 수량 및 금액을 반드시 확인하시기 바랍니다.\n유료재화로 구매된 상품은 구매 후 7일 이내에는 청약철회가 가능합니다.\n상품을 사용하였거나 즉시 적용되는 상품의 경우, 추가 혜택이 사용된 경우 청약 철회 대상에서 제외됩니다.\n(유료 재화부터 우선 소모 됩니다.)
                labelCoolingOff.Text = BasisUrl.KoreanPurchaseTerms.AppendText(LOCAL_KEY.ToText());
            }
            else
            {
                labelCoolingOff.Text = StringBuilderPool.Get()
                    .AppendLine(LocalizeKey._90220.ToText()) // 구매한 아이템은 우편함으로 지급 됩니다.\n우편함에서 해당 상품의 구성품을 수령 할 경우, 청약 철회가 불가능합니다.
                    .Append(LocalizeKey._4605.ToText()) // 제공되는 OnBuff 포인트가 모두 소진시 OnBuff 포인트 제공이 중단됩니다.
                    .Release();
            }

            labelRewardTitle.LocalKey = LocalizeKey._4602; // 패스한정 특권 발동
            labelRewardValue1.LocalKey = LocalizeKey._4603; // 일일 퀘스트를 통한 추가 포인트 제공!
            labelRewardValue2.LocalKey = LocalizeKey._4604; // MVP 처치를 통해 추가 포인트 제공!
            labelAccountConnectNotice.LocalKey = LocalizeKey._90333; // ONBUFF 포인트는 INNO 지갑 연동 후 받기가 가능합니다.
        }

        void CloseUI()
        {
            UI.Close<UIPackageOnBuffPass>();
        }

        void Refresh()
        {
            labelTitle.Text = presenter.GetTitle();
            labelRewardDesc.Text = presenter.GetDescription();
            SetPrice();
            SetRemainTime(presenter.GetRemainTime());
            SetJobLevelLimit();
        }

        public void SetRemainTime(RemainTime remainTime)
        {
            Timing.RunCoroutineSingleton(YieldRemainTime(remainTime).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private IEnumerator<float> YieldRemainTime(RemainTime remainTime)
        {
            while (true)
            {
                float time = remainTime.ToRemainTime();

                if (time <= 0)
                    break;

                labelRemainTime.Text = LocalizeKey._4601.ToText().Replace(ReplaceKey.TIME, time.ToStringTimeConatinsDay()); // 남은 시즌 기간 {TIME}
                yield return Timing.WaitForSeconds(0.1f);
            }
            labelRemainTime.Text = LocalizeKey._4601.ToText().Replace(ReplaceKey.TIME, "00:00:00"); // 남은 시즌 기간 {TIME}
            SetPrice();
        }

        /// <summary>
        /// 직업 레벨 제한 세팅
        /// </summary>
        private void SetJobLevelLimit()
        {
            int jobLevel = presenter.GetPlayerJobLevelValue();
            int jobLevelLimit = presenter.GetJobLevelLimitValue();
            bool isNotEnoughLevel = jobLevel < jobLevelLimit;

            if (isNotEnoughLevel)
            {
                labelJobLevel.Text = LocalizeKey._4606.ToText()
                    .Replace(ReplaceKey.LEVEL, jobLevelLimit); // 구매 제한 Job Lv {LEVEL}
            }
            labelJobLevel.SetActive(isNotEnoughLevel);
        }

        private void SetPrice()
        {
            // 구매 가능여부
            PassState canBuy = presenter.CanBuy();

            switch (canBuy)
            {
                case PassState.Avalable:
                    {
                        btnPurchase.IsEnabled = true;
                        btnPurchase.SetMode(UIGraySprite.SpriteMode.None);
                        btnPurchase.Text = presenter.GetPrice();
                    }
                    break;
                case PassState.LevelLimit:
                    {
                        btnPurchase.IsEnabled = false;
                        btnPurchase.SetMode(UIGraySprite.SpriteMode.Grayscale);
                        btnPurchase.Text = presenter.GetPrice();
                    }
                    break;
                case PassState.NotTime:
                    {
                        btnPurchase.IsEnabled = false;
                        btnPurchase.SetMode(UIGraySprite.SpriteMode.Grayscale);
                        btnPurchase.LocalKey = LocalizeKey._4701; // 구매 불가
                    }
                    break;
                case PassState.Disable:
                    {
                        btnPurchase.IsEnabled = false;
                        btnPurchase.SetMode(UIGraySprite.SpriteMode.Grayscale);
                        btnPurchase.LocalKey = LocalizeKey._4701; // 구매 불가
                    }
                    break;
                case PassState.OnActivate:
                case PassState.Purchased:
                    {
                        btnPurchase.IsEnabled = false;
                        btnPurchase.SetMode(UIGraySprite.SpriteMode.Grayscale);
                        btnPurchase.LocalKey = LocalizeKey._4600; // 구매 완료
                    }
                    break;
            }
        }
    }
}