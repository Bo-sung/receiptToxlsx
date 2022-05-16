using UnityEngine;

namespace Ragnarok.View
{
    public class GuildAttackChangeTimeHelpView : UIView
    {
        [SerializeField] PopupView changeTimePopupView;
        [SerializeField] UILabelHelper labelDescription;

        protected override void Awake()
        {
            base.Awake();
            changeTimePopupView.OnConfirm += Hide;
            changeTimePopupView.OnExit += Hide;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            changeTimePopupView.OnConfirm -= Hide;
            changeTimePopupView.OnExit -= Hide;
        }

        protected override void OnLocalize()
        {
            changeTimePopupView.MainTitleLocalKey = LocalizeKey._38414; // 길드 습격 시간 변경
            changeTimePopupView.ConfirmLocalKey = LocalizeKey._38416; // 확인
        }

        public void SetText(string text)
        {
            labelDescription.Text = text;
        }
    }
}