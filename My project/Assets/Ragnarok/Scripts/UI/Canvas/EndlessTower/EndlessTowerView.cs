using UnityEngine;

namespace Ragnarok.View
{
    public class EndlessTowerView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIEndlessTowerElement element;

        private SuperWrapContent<UIEndlessTowerElement, UIEndlessTowerElement.IInput> wrapContent;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UIEndlessTowerElement, UIEndlessTowerElement.IInput>(element);
        }

        protected override void OnLocalize()
        {
        }

        public void SetData(int clearedFloor, UIEndlessTowerElement.IInput[] inputs)
        {
            // ClearedFloor 먼저 세팅
            foreach (UIEndlessTowerElement item in wrapContent)
            {
                item.SetClearedFloor(clearedFloor);
            }

            wrapContent.SetData(inputs);
        }

        public void Move(int floor)
        {
            wrapContent.Move(floor - 1);
        }
    }
}