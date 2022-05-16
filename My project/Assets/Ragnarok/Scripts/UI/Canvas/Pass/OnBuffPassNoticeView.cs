using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UIOnBuffPass"/>
    /// </summary>
    public class OnBuffPassNoticeView : SelectPopupView
    {
        [SerializeField] UILabelHelper labelMessage;
        [SerializeField] UIButtonHelper btnBuyPass;
        [SerializeField] UILabelHelper labelNotice;

        public event System.Action OnSelectBuyPass;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnBuyPass.OnClick, OnClickedBtnBuyPass);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnBuyPass.OnClick, OnClickedBtnBuyPass);
        }

        protected override void OnLocalize()
        {
            labelMessage.LocalKey = LocalizeKey._39829; // OnBuff 패스를 구매하지 않았습니다.\n추가 포인트 없이 진행하시겠습니까?
            labelNotice.LocalKey = LocalizeKey._39833; // OnBuff 패스를 구매하면 추가 포인트를 획득할 수 있습니다.
            btnBuyPass.LocalKey = LocalizeKey._39819; // OnBuff 패스 구매
        }

        void OnClickedBtnBuyPass()
        {
            OnSelectBuyPass?.Invoke();
            Hide();
        }

        protected override void OnClickedBtnConfirm()
        {
            base.OnClickedBtnConfirm();
            Hide();
        }

        protected override void OnClickedBtnCancel()
        {
            base.OnClickedBtnCancel();
            Hide();
        }
    }
}