using UnityEngine;

namespace Ragnarok.View
{
    public sealed class GuildBattleRewardView : UIView
    {
        [SerializeField] UILabelValue header;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIRankRewardElement element;
        [SerializeField] UILabelHelper labelNotice;

        private SuperWrapContent<UIRankRewardElement, UIRankRewardElement.IInput> wrapContent;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UIRankRewardElement, UIRankRewardElement.IInput>(element);
        }

        protected override void OnLocalize()
        {
            header.ValueKey = LocalizeKey._33920; // 보상
        }

        public void SetTitleKey(int localKey)
        {
            header.TitleKey = localKey;
        }

        public void SetNoticeKey(int localKey)
        {
            labelNotice.LocalKey = localKey;
        }

        public void SetData(UIRankRewardElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
            wrapContent.SetProgress(0F);
        }
    }
}