using UnityEngine;

namespace Ragnarok.View
{
    public class RewardListView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIRewardListElement element;

        private SuperWrapContent<UIRewardListElement, UIRewardListElement.IInput> wrapContent;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UIRewardListElement, UIRewardListElement.IInput>(element);
        }

        protected override void OnLocalize()
        {
        }

        public void SetData(UIRewardListElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }
    }
}