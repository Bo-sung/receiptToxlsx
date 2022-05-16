using UnityEngine;

namespace Ragnarok.View
{
    public sealed class GuildBattleHistoryView : UIView
    {
        [Header("MyGuild")]
        [SerializeField] UILabelHelper labelMyGuild;
        [SerializeField] UISingleGuildBattleElement myGuild;
        [SerializeField] UIButtonHelper btnDetail;

        [Header("History")]
        [SerializeField] UILabelHelper labelHistory;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIGuildHistoryElement element;
        [SerializeField] UILabelHelper labelNoData;

        SuperWrapContent<UIGuildHistoryElement, UIGuildHistoryElement.IInput> wrapContent;

        public event System.Action OnSelectDetail;
        public event System.Action<int, int> OnSelectAttackerInfo;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnDetail.OnClick, OnClickedBtnDetail);

            wrapContent = wrapper.Initialize<UIGuildHistoryElement, UIGuildHistoryElement.IInput>(element);
            foreach (var item in wrapContent)
            {
                item.OnSelectBtnInfo += OnSelectBtnInfo;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnDetail.OnClick, OnClickedBtnDetail);

            foreach (var item in wrapContent)
            {
                item.OnSelectBtnInfo -= OnSelectBtnInfo;
            }
        }

        protected override void OnLocalize()
        {
            labelMyGuild.LocalKey = LocalizeKey._33721; // 내 길드 정보
            labelHistory.LocalKey = LocalizeKey._33723; // 내 길드 수비 기록
            labelNoData.LocalKey = LocalizeKey._33725; // 길드 수비 기록이 없습니다.
            btnDetail.LocalKey = LocalizeKey._33737; // 자세히
        }

        void OnClickedBtnDetail()
        {
            OnSelectDetail?.Invoke();
        }

        void OnSelectBtnInfo(UIGuildHistoryElement.IInput input)
        {
            OnSelectAttackerInfo?.Invoke(input.Uid, input.Cid);
        }

        /// <summary>
        /// 길드 정보 세팅
        /// </summary>
        public void SetGuild(UISingleGuildBattleElement.IInput input)
        {
            myGuild.SetData(input);
        }

        /// <summary>
        /// 수비 기록 세팅
        /// </summary>
        public void SetHistories(UIGuildHistoryElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);

            int length = inputs == null ? 0 : inputs.Length;
            labelNoData.SetActive(length == 0);
        }
    }
}