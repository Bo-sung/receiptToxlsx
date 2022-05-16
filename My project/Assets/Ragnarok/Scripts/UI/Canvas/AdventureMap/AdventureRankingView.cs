using UnityEngine;

namespace Ragnarok.View
{
    public class AdventureRankingView : UIView
    {
        [SerializeField] UILabelHelper labelMyRank;
        [SerializeField] AdventureRankingElement myRank;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] AdventureRankingElement element;
        [SerializeField] UILabelHelper labelNoData;

        private SuperWrapContent<AdventureRankingElement, AdventureRankingElement.IInput> wrapContent;

        public event System.Action OnDragFinish;
        public event AdventureRankingElement.SelectCharacterEvent OnSelect;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<AdventureRankingElement, AdventureRankingElement.IInput>(element);
            wrapContent.OnDragFinished += OnDragFinished;
            foreach (var item in wrapContent)
            {
                item.OnSelect += OnSelectElement;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            wrapContent.OnDragFinished -= OnDragFinished;
            foreach (var item in wrapContent)
            {
                item.OnSelect -= OnSelectElement;
            }
        }

        protected override void OnLocalize()
        {
            labelMyRank.LocalKey = LocalizeKey._47922; // 나의 순위
            labelNoData.LocalKey = LocalizeKey._47924; // 순위 정보가 없습니다.
        }

        void OnDragFinished()
        {
            // UIScrollView 408줄 참고
            Bounds b = wrapper.ScrollView.bounds;
            Vector3 constraint = wrapper.Panel.CalculateConstrainOffset(b.min, b.max);
            constraint.x = 0f;

            // 최하단에서 드래그 끝났을 때
            if (constraint.sqrMagnitude > 0.1f && constraint.y < 0f)
            {
                OnDragFinish?.Invoke();
            }
        }

        void OnSelectElement(int uid, int cid)
        {
            OnSelect?.Invoke(uid, cid);
        }

        public void SetMyRank(AdventureRankingElement.IInput input)
        {
            myRank.SetData(input);
        }

        public void SetData(AdventureRankingElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);

            int dataSize = inputs == null ? 0 : inputs.Length;
            labelNoData.SetActive(dataSize == 0);
        }
    }
}