using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIAdConsent : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButton btnClose;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelHelper labelTitle;

        [SerializeField] UILabelHelper labelDesc;

        [SerializeField] UIButtonHelper btnAdConsent;
        [SerializeField] GameObject adConsentOff;
        [SerializeField] GameObject adConsentOn;

        [SerializeField] UIButtonHelper btnYes;
        [SerializeField] UIButtonHelper btnNo;

        bool isAgree;

        public static string AdConsentKey = "AdConsentActive";


        protected override void OnInit()
        {
            EventDelegate.Add(btnClose.onClick, CancelAgree);
            EventDelegate.Add(btnExit.OnClick, CancelAgree);

            EventDelegate.Add(btnAdConsent.OnClick, ToggleBtnAgree);

            EventDelegate.Add(btnNo.OnClick, CancelAgree);
            EventDelegate.Add(btnYes.OnClick, ConfirmAgree);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnClose.onClick, CancelAgree);
            EventDelegate.Remove(btnExit.OnClick, CancelAgree);
            
            EventDelegate.Remove(btnAdConsent.OnClick, ToggleBtnAgree);

            EventDelegate.Remove(btnNo.OnClick, CancelAgree);
            EventDelegate.Remove(btnYes.OnClick, ConfirmAgree);
        }

        protected override void OnShow(IUIData data = null)
        {
            isAgree = true;
            RefreshCheckBtn();
        }

        protected override void OnHide()
        {

        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._14400; // 광고 개인화 동의
            labelDesc.LocalKey = LocalizeKey._14401; // 개인 선호도에 따라 게임에서 타겟팅 된 광고를 제공 할 목적으로\n기기에서 광고 ID를 수집합니다.\n개인화 동의에 대한 변경은 앱 설정에서\n언제든지 변경할 수 있습니다.
            btnAdConsent.LocalKey = LocalizeKey._14402; // 위 항목에 동의 합니다.
            btnNo.LocalKey = LocalizeKey._2; // 취소
            btnYes.LocalKey = LocalizeKey._1; // 확인
        }

        void ToggleBtnAgree()
        {
            isAgree = !isAgree;
            RefreshCheckBtn();
        }

        void RefreshCheckBtn()
        {
            btnYes.IsEnabled = isAgree;
            adConsentOff.SetActive(!isAgree);
            adConsentOn.SetActive(isAgree);
        }

        void ConfirmAgree()
        {
            // 동의 상태값 true
            GameAdSettings.Instance.GameAdAppCustomType = AdAppCustomType.On;

            CloseUI();
        }

        void CancelAgree()
        {
            // 동의 상태값 false
            GameAdSettings.Instance.GameAdAppCustomType = AdAppCustomType.Off;

            CloseUI();
        }

        void CloseUI()
        {
            UI.Close<UIAdConsent>();
        }
    }
}