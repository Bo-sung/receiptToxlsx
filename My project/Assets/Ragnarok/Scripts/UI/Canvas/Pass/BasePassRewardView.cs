using UnityEngine;

namespace Ragnarok.View
{
    public abstract class BasePassRewardView : UIView
    {
        [SerializeField] protected UILabelHelper labelFree, labelPass;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIBasePassRewardElement element;

        private SuperWrapContent<UIBasePassRewardElement, UIBasePassRewardElement.IInput> wrapContent;

        public event System.Action OnSelectBuyExp;
        public event System.Action<(byte passFlag, int level)> OnSelectReceive; // 1 : 무료 , 2: 유료

        protected override void Awake()
        {
            base.Awake();
            wrapContent = wrapper.Initialize<UIBasePassRewardElement, UIBasePassRewardElement.IInput>(element);
            foreach (var item in wrapContent)
            {
                item.OnSelectBuyExp += InvokeSelectBuyExp;
                item.OnSelectReceive += InvokeSelectReceive;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var item in wrapContent)
            {
                item.OnSelectBuyExp -= InvokeSelectBuyExp;
                item.OnSelectReceive -= InvokeSelectReceive;
            }
        }

        protected override void OnLocalize()
        {
            labelFree.LocalKey = LocalizeKey._39807; // 일반
        }

        public void SetData(UIBasePassRewardElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }

        public void MoveTo(int index)
        {
            wrapContent.Move(index);
        }

        void InvokeSelectBuyExp()
        {
            OnSelectBuyExp?.Invoke();
        }

        void InvokeSelectReceive((byte passFlag, int level) item)
        {
            OnSelectReceive?.Invoke(item);
        }
    }
}