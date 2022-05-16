using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIPopupOnBuffUnLink : UIView
    {
        [SerializeField] UIButton btnClose;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButton btnExit;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelHelper labelInnoUIDTitle;
        [SerializeField] UILabelHelper labelInnoUID;
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnUnLink;

        public event System.Action OnClickedBtnUnLink;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnClose.onClick, Hide);
            EventDelegate.Add(btnExit.onClick, Hide);
            EventDelegate.Add(btnCancel.OnClick, Hide);
            EventDelegate.Add(btnUnLink.OnClick, InvokeBtnUnLink);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnClose.onClick, Hide);
            EventDelegate.Remove(btnExit.onClick, Hide);
            EventDelegate.Remove(btnCancel.OnClick, Hide);
            EventDelegate.Remove(btnUnLink.OnClick, InvokeBtnUnLink);
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._14060; // 연동해제
            labelDescription.LocalKey = LocalizeKey._14061; // [3C3A3D]연동을 해제 하시겠습니까?[-]\n\n[AFAFAF]연동을 해제 할 경우 연동해제 한 INNO 계정은\n24시간 동안 다른 계정과 연동 할 수 없습니다.[-]
            labelInnoUIDTitle.LocalKey = LocalizeKey._14062; // 현재 연동된 INNO UID
            btnCancel.LocalKey = LocalizeKey._2; // 취소
            btnUnLink.LocalKey = LocalizeKey._14063; // 연동해제
        }

        public void SetInnoUID(string innoUID)
        {
            labelInnoUID.Text = innoUID;
        }

        private void InvokeBtnUnLink()
        {
            OnClickedBtnUnLink?.Invoke();
        }
    }
}