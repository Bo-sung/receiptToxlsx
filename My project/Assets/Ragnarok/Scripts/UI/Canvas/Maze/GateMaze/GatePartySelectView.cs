using UnityEngine;

namespace Ragnarok.View
{
    public sealed class GatePartySelectView : UIView
    {
        public enum SelectType { Exit, Create, Join, Help, }

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnExit;
        [SerializeField] UIButtonHelper btnCreate, btnJoin;
        [SerializeField] UILabelHelper labelTicketCount;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIButton btnHelp;

        public event System.Action<SelectType> OnSelect;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnExit.onClick, OnClickedBtnExit);
            EventDelegate.Add(btnCreate.OnClick, OnClickedBtnCreate);
            EventDelegate.Add(btnJoin.OnClick, OnClickedBtnJoin);
            EventDelegate.Add(btnHelp.onClick, OnClickedBtnHelp);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnExit.onClick, OnClickedBtnExit);
            EventDelegate.Remove(btnCreate.OnClick, OnClickedBtnCreate);
            EventDelegate.Remove(btnJoin.OnClick, OnClickedBtnJoin);
            EventDelegate.Remove(btnHelp.onClick, OnClickedBtnHelp);
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._6900; // 파티 선택
            btnCreate.LocalKey = LocalizeKey._6901; // 파티 생성
            btnJoin.LocalKey = LocalizeKey._6902; // 파티 참가
            labelDescription.LocalKey = LocalizeKey._6903; // 동료들과 파티를 맺어 협동할 수 있습니다.\n중간보스를 모두 처치하여 최종 보스에 도전하세요.
        }

        void OnClickedBtnExit()
        {
            OnSelect?.Invoke(SelectType.Exit);
        }

        void OnClickedBtnCreate()
        {
            OnSelect?.Invoke(SelectType.Create);
        }

        void OnClickedBtnJoin()
        {
            OnSelect?.Invoke(SelectType.Join);
        }

        void OnClickedBtnHelp()
        {
            OnSelect?.Invoke(SelectType.Help);
        }

        public void UpdateTicketCount(int curCount, int maxCount)
        {
            labelTicketCount.Text = StringBuilderPool.Get()
                .Append(curCount).Append('/').Append(maxCount)
                .Release();
        }

        #region Tutorial
        public UIWidget GetBtnCreateWidget()
        {
            return btnCreate.GetComponent<UIWidget>();
        }

        public UIWidget GetBtnJoinWidget()
        {
            return btnJoin.GetComponent<UIWidget>();
        }
        #endregion
    }
}