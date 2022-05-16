using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIPopupOnBuffLink : UIView
    {
        [SerializeField] UIButton btnClose;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIButton btnExit;
        [SerializeField] UIInput inputName;
        [SerializeField] UIButtonHelper btnLink;
        [SerializeField] UILabelHelper labelUIDLink;

        public event System.Action<string> OnClickedBtnLink;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnClose.onClick, Hide);
            EventDelegate.Add(btnExit.onClick, Hide);
            EventDelegate.Add(inputName.onChange, OnChangeInput);
            EventDelegate.Add(btnLink.OnClick, InvokeBtnLink);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnClose.onClick, Hide);
            EventDelegate.Remove(btnExit.onClick, Hide);
            EventDelegate.Remove(inputName.onChange, OnChangeInput);
            EventDelegate.Remove(btnLink.OnClick, InvokeBtnLink);
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._14057; // INNO UID 입력
            btnLink.LocalKey = LocalizeKey._14059; // 연동하기
            inputName.defaultText = LocalizeKey._14058.ToText(); // UID를 입력해주세요.
            labelDescription.LocalKey = LocalizeKey._14066; // UID 확인을 위해 INNO Platform으로 이동이 필요할 경우\n우측 하단 링크를 통해 이동이 가능합니다.
            labelUIDLink.Text = BasisUrl.OnBuffHompage.AppendText(string.Empty, useColor: true);
        }

        public override void Show()
        {
            base.Show();
            inputName.value = string.Empty;
            OnChangeInput();
        }

        private void OnChangeInput()
        {
            btnLink.IsEnabled = !string.IsNullOrEmpty(inputName.value);
        }

        private void InvokeBtnLink()
        {
            OnClickedBtnLink?.Invoke(inputName.value);
        }
    }
}