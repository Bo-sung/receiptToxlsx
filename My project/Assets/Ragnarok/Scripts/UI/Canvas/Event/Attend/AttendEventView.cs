using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UIAttendEvent"/>
    /// </summary>
    public class AttendEventView : UIView
    {
        [SerializeField] UIBannerElement banner;
        [SerializeField] UIAttendEventElement[] elements;

        public event System.Action OnSelect;

        protected override void Awake()
        {
            base.Awake();
            foreach (var item in elements)
            {
                item.OnSelect += OnSelectElement;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var item in elements)
            {
                item.OnSelect -= OnSelectElement;
            }
        }

        protected override void OnLocalize()
        {
        }

        public void SetData(UIBannerElement.IInput input)
        {
            banner.SetData(input);
        }

        public void SetData(UIAttendEventElement.IInput[] inputs)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].SetData(i < inputs.Length ? inputs[i] : null);
            }
        }

        private void OnSelectElement()
        {
            OnSelect?.Invoke();
        }
    }
}