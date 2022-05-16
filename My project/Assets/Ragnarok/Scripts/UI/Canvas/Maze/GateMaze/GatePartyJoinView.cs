using UnityEngine;

namespace Ragnarok.View
{
    public sealed class GatePartyJoinView : UIView
    {
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnExit;

        [Header("TitleInfo")]
        [SerializeField] UITextureHelper iconStage;
        [SerializeField] UILabelValue title;
        [SerializeField] UITextureHelper iconMonster;

        [Header("ListView")]
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIGatePartyJoinElement element;
        [SerializeField] UILabelHelper labelNoData;

        [Header("Bottom")]
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UIButtonHelper btnClose, btnRefresh;

        private SuperWrapContent<UIGatePartyJoinElement, UIGatePartyJoinElement.IInput> wrapContent;

        public event System.Action OnSelectRefresh;
        public event UserModel.UserInfoEvent OnSelectUserInfo;
        public event UIGatePartyJoinSlot.JoinEvent OnSelectJoin;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnExit.onClick, Hide);
            EventDelegate.Add(btnClose.OnClick, Hide);
            EventDelegate.Add(btnRefresh.OnClick, OnClickedBtnRefresh);

            wrapContent = wrapper.Initialize<UIGatePartyJoinElement, UIGatePartyJoinElement.IInput>(element);

            foreach (var item in wrapContent)
            {
                item.OnSelectUserInfo += OnUserInfo;
                item.OnSelectJoin += OnJoin;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnExit.onClick, Hide);
            EventDelegate.Remove(btnClose.OnClick, Hide);
            EventDelegate.Remove(btnRefresh.OnClick, OnClickedBtnRefresh);

            foreach (var item in wrapContent)
            {
                item.OnSelectUserInfo -= OnUserInfo;
                item.OnSelectJoin -= OnJoin;
            }
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._6904; // 파티 가입
            btnClose.LocalKey = LocalizeKey._4100; // 닫기
            btnRefresh.LocalKey = LocalizeKey._6907; // 목록 변경
            labelNoData.LocalKey = LocalizeKey._6908; // 생성된 파티가 없습니다.
            labelNotice.LocalKey = LocalizeKey._6914; // 참가한 파티원이 많을수록 더 많은 보상을 얻을 수 있습니다.
        }

        void OnClickedBtnRefresh()
        {
            OnSelectRefresh?.Invoke();
        }

        void OnUserInfo(int uid, int cid)
        {
            OnSelectUserInfo?.Invoke(uid, cid);
        }

        void OnJoin(int channelId)
        {
            OnSelectJoin?.Invoke(channelId);
        }

        public void Initialize(int titleLocalKey, int tileValueLocalKey, string stageIconName, string monsterIconName)
        {
            title.TitleKey = titleLocalKey;
            title.ValueKey = tileValueLocalKey;
            iconStage.SetAdventure(stageIconName);
            iconMonster.SetMonster(monsterIconName);
        }

        public void SetData(UIGatePartyJoinElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);

            int length = inputs == null ? 0 : inputs.Length;
            labelNoData.SetActive(length == 0);
        }
    }
}