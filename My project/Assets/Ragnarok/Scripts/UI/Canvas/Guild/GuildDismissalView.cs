using UnityEngine;

namespace Ragnarok.View
{
    public class GuildDismissalView : UIView
    {
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnExit;
        [SerializeField] UILabelHelper labelMessage, labelDescription, labelNotice;
        [SerializeField] UIButtonHelper btnClose, btnDismissal;

        private int guildMasterDismissalDays;

        public event System.Action OnSelectExit, OnSelectMasterDismissal;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnExit.onClick, SelectExit);
            EventDelegate.Add(btnClose.OnClick, SelectExit);
            EventDelegate.Add(btnDismissal.OnClick, OnClickedBtnDismissal);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnExit.onClick, SelectExit);
            EventDelegate.Remove(btnClose.OnClick, SelectExit);
            EventDelegate.Remove(btnDismissal.OnClick, OnClickedBtnDismissal);
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._33118; // 길드장 해임
            labelMessage.Text = LocalizeKey._33119.ToText() // [c][84A2EC]{DAYS}일 미접속 시[-][/c] 길드장 해임 버튼이\n활성화 되며 부길드장만이 해임 가능합니다.
                .Replace(ReplaceKey.DAYS, guildMasterDismissalDays);
            labelDescription.LocalKey = LocalizeKey._33120; // 해임 버튼을 누른 부길드장이 길드장이 됩니다.
            labelNotice.LocalKey = LocalizeKey._33121; // (해임된 길드장은 길드원으로 자동 변경됩니다.)
            btnClose.LocalKey = LocalizeKey._4100; // 닫기
            btnDismissal.LocalKey = LocalizeKey._33122; // 길드장 해임
        }

        void SelectExit()
        {
            OnSelectExit?.Invoke();
        }

        void OnClickedBtnDismissal()
        {
            OnSelectMasterDismissal?.Invoke();
        }

        public void Initialize(int guildMasterDismissalDays)
        {
            this.guildMasterDismissalDays = guildMasterDismissalDays;
        }

        public void SetMasterDismissal(bool isEnable)
        {
            btnDismissal.IsEnabled = isEnable;
        }
    }
}