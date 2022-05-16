using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UITranscendenceDisassemble"/>
    /// </summary>
    public class DisassembleListView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIDisassembleElement element;
        [SerializeField] UILabelHelper labelNoData;

        private SuperWrapContent<UIDisassembleElement, UIDisassembleElement.IInput> wrapContent;

        public event System.Action<long> OnSelect;

        protected override void Awake()
        {
            base.Awake();
            wrapContent = wrapper.Initialize<UIDisassembleElement, UIDisassembleElement.IInput>(element);
            foreach (var item in wrapContent)
            {
                item.OnSelect += InvokeOnSelect;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var item in wrapContent)
            {
                item.OnSelect -= InvokeOnSelect;
            }
        }

        protected override void OnLocalize()
        {
            labelNoData.LocalKey = LocalizeKey._22300; // 해당 타입의 아이템이 없습니다.
        }

        public void SetData(UIDisassembleElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
            labelNoData.SetActive(inputs.Length == 0);
        }

        private void InvokeOnSelect(long itemNo)
        {
            OnSelect?.Invoke(itemNo);
        }
    }
}