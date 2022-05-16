using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UICupet"/>
    /// </summary>
    public class CupetListView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UICupetElement element;

        private SuperWrapContent<UICupetElement, UICupetElement.IInput> wrapContent;

        public event System.Action<int> OnSelect;

        protected override void Awake()
        {
            base.Awake();
            wrapContent = wrapper.Initialize<UICupetElement, UICupetElement.IInput>(element);
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
        }

        public void SetData(UICupetElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }

        void OnSelectElement(int cupetId)
        {
            OnSelect?.Invoke(cupetId);
        }
    }
}