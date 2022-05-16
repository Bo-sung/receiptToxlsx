using UnityEngine;

namespace Ragnarok.View
{
    public sealed class DuelArenaRewardView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIDuelReward element;

        private SuperWrapContent<UIDuelReward, UIDuelReward.IInput> wrapContent;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UIDuelReward, UIDuelReward.IInput>(element);
        }

        protected override void OnLocalize()
        {
        }

        public void SetData(UIDuelReward.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }
    }
}