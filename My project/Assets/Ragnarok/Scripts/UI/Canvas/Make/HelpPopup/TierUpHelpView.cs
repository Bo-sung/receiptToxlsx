using UnityEngine;

namespace Ragnarok.View
{
    public class TierUpHelpView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UITierUpHelpElement element;
        [SerializeField] UILabelHelper labelDescription;

        private SuperWrapContent<UITierUpHelpElement, UITierUpHelpElement.IInput> wrapContent;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UITierUpHelpElement, UITierUpHelpElement.IInput>(element);
        }

        protected override void OnLocalize()
        {
            labelDescription.LocalKey = LocalizeKey._5503; // 5랭크 장비부터 초월이 가능하며 JOB Lv이 부족하면\n초월 및 장착이 불가능합니다.
        }

        public void SetData(UITierUpHelpElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }
    }
}