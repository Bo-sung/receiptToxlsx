using UnityEngine;

namespace Ragnarok.View
{
    public class GuideView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIGuideElement element;
        [SerializeField] UILabelHelper labelDescription;

        private SuperWrapContent<UIGuideElement, UIGuideElement.IInput> wrapContent;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UIGuideElement, UIGuideElement.IInput>(element);
        }

        protected override void OnLocalize()
        {
            labelDescription.LocalKey = LocalizeKey._5401; // 앞으로 체험할 수 있는 컨텐츠를 확인해보세요!
        }

        public void SetData(UIGuideElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }
    }
}