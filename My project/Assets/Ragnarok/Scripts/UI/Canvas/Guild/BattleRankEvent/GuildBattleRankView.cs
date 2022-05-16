using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UIGuildBattleRankEvent"/>
    /// </summary>
    public class GuildBattleRankView : UIView
    {
        [SerializeField] UILabelValue header;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIGuildBattleRankElement element;

        private SuperWrapContent<UIGuildBattleRankElement, UIGuildBattleRankElement.IInput> wrapContent;

        public event System.Action OnDragFinish;

        protected override void Awake()
        {
            base.Awake();
            wrapContent = wrapper.Initialize<UIGuildBattleRankElement, UIGuildBattleRankElement.IInput>(element);
            wrapContent.OnDragFinished += OnDragFinished;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            wrapContent.OnDragFinished -= OnDragFinished;
        }

        protected override void OnLocalize()
        {
            header.TitleKey = LocalizeKey._38602; // 순위
            header.ValueKey = LocalizeKey._38603; // 누적 피해량
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

        public void SetData(UIGuildBattleRankElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }
    }
}