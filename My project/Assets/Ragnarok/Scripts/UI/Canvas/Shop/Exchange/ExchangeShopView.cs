using UnityEngine;

namespace Ragnarok.View
{
    public class ExchangeShopView : UIView
    {
        [SerializeField] NPCStyle npc;
        [SerializeField] UILabelHelper labelNotice;

        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIExchangeElement element;

        private SuperWrapContent<UIExchangeElement, UIExchangeElement.IInput> wrapContent;

        public event System.Action<int, int> OnSelect;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UIExchangeElement, UIExchangeElement.IInput>(element);

            foreach (var item in wrapContent)
            {
                item.OnSelect += OnSelectElement;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in wrapContent)
            {
                item.OnSelect -= OnSelectElement;
            }
        }

        protected override void OnLocalize()
        {
            labelNotice.LocalKey = LocalizeKey._8066; // 카프라 아이템은 주로 상점에서 획득할 수 있습니다.
        }

        public void SetData(UIExchangeElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }

        void OnSelectElement(int id, int count)
        {
            OnSelect?.Invoke(id, count);
        }
    }
}