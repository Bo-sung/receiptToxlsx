using UnityEngine;

namespace Ragnarok.View
{
    public sealed class GateRewardSlot : UIView, IInspectorFinder
    {
        private enum Type
        {
            Free,
            One,
            Two,
            Three,
            Four,
        }

        [SerializeField, EnumIndex(typeof(Type))] int index;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIGateRewardElement element;

        private SuperWrapContent<UIGateRewardElement, RewardData> wrapContent;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UIGateRewardElement, RewardData>(element);
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = GetLocalKey();
        }

        public void SetData(RewardData[] inputs)
        {
            wrapContent.SetData(inputs);
        }

        private int GetLocalKey()
        {
            switch (index)
            {
                case 0: return LocalizeKey._6952; // 기본 참여 보상
                case 1: return LocalizeKey._6953; // 1인 클리어 보상
                case 2: return LocalizeKey._6954; // 2인 클리어 보상
                case 3: return LocalizeKey._6955; // 3인 클리어 보상
                case 4: return LocalizeKey._6956; // 4인 클리어 보상
            }

            throw new System.ArgumentException($"유효하지 않은 처리: {nameof(index)} = {index}");
        }

        bool IInspectorFinder.Find()
        {
            int index = transform.GetSiblingIndex();
            if (this.index == index)
                return false;

            this.index = index;
            return true;
        }
    }
}