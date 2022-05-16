using UnityEngine;

namespace Ragnarok.View
{
    public class RankView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIRankElement element;

        private RankType rankType = RankType.All;

        private SuperWrapContent<UIRankElement, UIRankElement.IInput> wrapContent;

        public event System.Action<RankType> OnDragFinish;
        public event System.Action<UIRankElement.IInput> OnSelect;

        protected override void Awake()
        {
            base.Awake();
            wrapContent = wrapper.Initialize<UIRankElement, UIRankElement.IInput>(element);
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
        }

        public void SetRankType(RankType rankType)
        {
            this.rankType = rankType;
        }

        private void OnDragFinished()
        {
            // UIScrollView 408줄 참고
            Bounds b = wrapper.ScrollView.bounds;
            Vector3 constraint = wrapper.Panel.CalculateConstrainOffset(b.min, b.max);
            constraint.x = 0f;

            // 최하단에서 드래그 끝났을 때
            if (constraint.sqrMagnitude > 0.1f && constraint.y < 0f)
            {
                OnDragFinish?.Invoke(rankType);
            }
        }

        public void SetData(UIRankElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }

        public void SetProgress(float progress)
        {
            wrapContent.SetProgress(progress);
        }

        void OnSelectElement(UIRankElement.IInput input)
        {
            OnSelect?.Invoke(input);
        }
    }
}