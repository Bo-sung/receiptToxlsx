using UnityEngine;

namespace Ragnarok.View
{
    public sealed class GatePartyReadyView : UIView
    {
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnExit;

        [Header("TitleInfo")]
        [SerializeField] UITextureHelper iconStage;
        [SerializeField] UILabelValue title;
        [SerializeField] UITextureHelper iconMonster;

        [Header("ReadyInfo")]
        [SerializeField] UIGatePartyReadySlot[] slots;

        [Header("Bottom")]
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UIButtonHelper btnClose, btnStart;

        public event System.Action OnSelectExit;
        public event System.Action OnSelectStart;
        public event UserModel.UserInfoEvent OnSelectUserInfo;
        public event UIGatePartyReadySlot.BanEvent OnSelectUserBan;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnExit.onClick, TryExit);
            EventDelegate.Add(btnClose.OnClick, TryExit);
            EventDelegate.Add(btnStart.OnClick, OnClickedBtnStart);

            foreach (var item in slots)
            {
                item.OnSelectUserInfo += OnUserInfo;
                item.OnSelectUserBan += OnUserBan;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnExit.onClick, TryExit);
            EventDelegate.Remove(btnClose.OnClick, TryExit);
            EventDelegate.Remove(btnStart.OnClick, OnClickedBtnStart);

            foreach (var item in slots)
            {
                item.OnSelectUserInfo -= OnUserInfo;
                item.OnSelectUserBan -= OnUserBan;
            }
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._6911; // 파티 준비
            btnClose.LocalKey = LocalizeKey._6912; // 닫기
            btnStart.LocalKey = LocalizeKey._6913; // 시작
            labelNotice.LocalKey = LocalizeKey._6914; // 참가한 파티원이 많을수록 더 많은 보상을 얻을 수 있습니다.
        }

        void OnClickedBtnStart()
        {
            OnSelectStart?.Invoke();
        }

        void OnUserInfo(int uid, int cid)
        {
            OnSelectUserInfo?.Invoke(uid, cid);
        }

        void OnUserBan(int cid)
        {
            OnSelectUserBan?.Invoke(cid);
        }

        public void Initialize(int titleLocalKey, int tileValueLocalKey, string stageIconName, string monsterIconName)
        {
            title.TitleKey = titleLocalKey;
            title.ValueKey = tileValueLocalKey;
            iconStage.SetAdventure(stageIconName);
            iconMonster.SetMonster(monsterIconName);
        }

        public void SetData(UIGatePartyReadySlot.IInput[] inputs)
        {
            int inputLength = inputs == null ? 0 : inputs.Length;
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].SetData(i < inputLength ? inputs[i] : null);
            }
        }

        public void SetLeader(bool isLeader)
        {
            btnStart.IsEnabled = isLeader;

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].SetLeader(isLeader);
            }
        }

        public void TryExit()
        {
            OnSelectExit?.Invoke();
        }
    }
}