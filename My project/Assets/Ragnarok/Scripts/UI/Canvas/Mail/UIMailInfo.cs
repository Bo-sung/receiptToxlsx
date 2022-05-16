using UnityEngine;

namespace Ragnarok
{
    public class UIMailInfo : UIInfo<MailPresenter, MailInfo>
    {
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UILabelHelper labelTitle, labelDesc, labelDate;
        [SerializeField] UIButtonHelper btnGet;
        [SerializeField] UIButtonHelper btnGetAd; // 널체크
        [SerializeField] UIButtonHelper btnGetFB; // 널체크
        [SerializeField] UIButtonHelper btnViewMsg; // 널체크
        [SerializeField] GameObject goIconGM;

        // 거래소 탭 전용
        [SerializeField] UIButtonHelper btnDone;

        // 상점 탭 전용
        [SerializeField] UITextureHelper iconShopMail;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnGet.OnClick, OnClickedBtnGet);

            if (btnGetAd)
            {
                EventDelegate.Add(btnGetAd.OnClick, OnClickedBtnGetAd);
            }

            if (btnGetFB)
            {
                EventDelegate.Add(btnGetFB.OnClick, OnClickedBtnGetFB);
            }

            if (btnViewMsg)
            {
                EventDelegate.Add(btnViewMsg.OnClick, OnClickedBtnViewMsg);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnGet.OnClick, OnClickedBtnGet);

            if (btnGetAd)
            {
                EventDelegate.Remove(btnGetAd.OnClick, OnClickedBtnGetAd);
            }

            if (btnGetFB)
            {
                EventDelegate.Remove(btnGetFB.OnClick, OnClickedBtnGetFB);
            }

            if (btnViewMsg)
            {
                EventDelegate.Remove(btnViewMsg.OnClick, OnClickedBtnViewMsg);
            }
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();

            btnGet.LocalKey = LocalizeKey._12003; // 받기
            if (btnDone != null)
                btnDone.LocalKey = LocalizeKey._12007; // 수령 완료

            if (btnGetAd)
            {
                btnGetAd.LocalKey = LocalizeKey._12003; // 받기
            }

            if (btnGetFB)
            {
                btnGetFB.LocalKey = LocalizeKey._12003; // 받기
            }

            if (btnViewMsg)
            {
                btnViewMsg.LocalKey = LocalizeKey._12024; // 보기
            }
        }

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            MailType mailType = presenter.GetCurMailType();

            switch (mailType)
            {
                case MailType.Account:
                case MailType.Character:
                case MailType.OnBuff:
                    SetMail();
                    break;

                case MailType.Shop:
                    SetShopMail();
                    break;

                case MailType.Trade:
                    SetTradeMail();
                    break;
            }
        }

        /// <summary>
        /// 일반적인(계정,캐릭터) 우편함
        /// </summary>
        private void SetMail()
        {
            if (info.MailGroup == MailGroup.PolicyViolation || info.MailGroup == MailGroup.OnlyMessage)
            {
                if (goIconGM)
                    goIconGM.SetActive(true);
                rewardHelper.SetData(null);
            }
            else
            {
                if (goIconGM)
                    goIconGM.SetActive(false);
                rewardHelper.SetData(info.GetReward());
                rewardHelper.UseDefaultButtonEvent = true;
            }

            labelTitle.Text = info.GetTitle();
            labelDesc.Text = info.GetMessage();
            labelDate.Text = info.GetDate();

            btnGet.SetActive(info.MailGroup == MailGroup.Normal);

            if (btnGetAd)
            {
                btnGetAd.SetActive(info.MailGroup == MailGroup.Advertisement);
            }

            if (btnGetFB)
            {
                btnGetFB.SetActive(info.MailGroup == MailGroup.Facebook);
            }

            if (btnViewMsg)
            {
                btnViewMsg.SetActive(info.MailGroup == MailGroup.PolicyViolation || info.MailGroup == MailGroup.OnlyMessage);
            }
        }

        private void SetShopMail()
        {
            labelTitle.Text = info.GetMessage();
            rewardHelper.SetData(null);
            iconShopMail.Set(presenter.GetMailShopIconName(info.GetShopId()));
            labelDesc.LocalKey = LocalizeKey._12015; // 상점에서 구매한 상품이 도착했습니다.
            labelDate.Text = info.GetDate();
            btnGet.SetActive(true);
        }

        /// <summary>
        /// 거래소 탭 우편함
        /// </summary>
        private void SetTradeMail()
        {
            labelTitle.Text = info.GetTitle();
            rewardHelper.SetData(info.GetReward());
            rewardHelper.UseDefaultButtonEvent = false;
            labelDesc.Text = info.GetMessageTrade();
            labelDate.Text = info.GetDate();

            bool isGetItem = info.isGetItem;
            btnDone.SetActive(isGetItem);
            btnGet.SetActive(!isGetItem);
        }

        /// <summary>
        /// 메일 받기 버튼 클릭
        /// </summary>
        void OnClickedBtnGet()
        {
            presenter.RequestReceiveMail(info).WrapNetworkErrors();
        }

        /// <summary>
        /// 광고보고 메일받기 버튼
        /// </summary>
        void OnClickedBtnGetAd()
        {
            presenter.RequestReceiveAdMail(info).WrapNetworkErrors();
        }

        /// <summary>        
        /// 페이지 방문하고(페북 좋아요) 메일받기 버튼
        /// </summary>
        void OnClickedBtnGetFB()
        {
            presenter.RequestReceiveFBMail(info).WrapNetworkErrors();
        }

        /// <summary>
        /// GM 메일 보기
        /// </summary>
        void OnClickedBtnViewMsg()
        {
            presenter.RequestReceiveMessageMail(info).WrapNetworkErrors();
        }
    }
}