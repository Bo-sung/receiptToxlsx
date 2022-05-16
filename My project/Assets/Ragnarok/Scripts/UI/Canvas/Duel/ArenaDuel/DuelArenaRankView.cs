using UnityEngine;

namespace Ragnarok.View
{
    public sealed class DuelArenaRankView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIRankElement element;
        [SerializeField] UILabelHelper labelNoData;
        [SerializeField] UIRankElement myRankElement;

        private SuperWrapContent<UIRankElement, UIRankElement.IInput> wrapContent;

        public event UserModel.UserInfoEvent OnSelectUserInfo;
        public event System.Action OnDragFinish;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UIRankElement, UIRankElement.IInput>(element);
            wrapContent.OnDragFinished += OnDragFinishedScrollList;

            foreach (var item in wrapContent)
            {
                item.OnSelect += OnSelect;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            wrapContent.OnDragFinished -= OnDragFinishedScrollList;
            foreach (var item in wrapContent)
            {
                item.OnSelect -= OnSelect;
            }
        }

        void OnSelect(UIRankElement.IInput input)
        {
            OnSelectUserInfo?.Invoke(input.UID, input.CID);
        }

        void OnDragFinishedScrollList()
        {
            OnDragFinish?.Invoke();
        }

        protected override void OnLocalize()
        {
            labelNoData.LocalKey = LocalizeKey._47932; // 순위 정보가 없습니다.
        }

        public void SetData(UIRankElement.IInput[] ranks, UIRankElement.IInput myRank)
        {
            int length = ranks == null ? 0 : ranks.Length;
            labelNoData.SetActive(length == 0);
            wrapContent.SetData(ranks);

            myRankElement.SetData(myRank);
        }
    }
}