using UnityEngine;

namespace Ragnarok.View
{
    public class ShareForceOptionSlot : UIView
    {
        [SerializeField] UILabelHelper labelOptionName;
        [SerializeField] UILabelHelper labelOptionValue;
        [SerializeField] UILabelHelper labelValue;
        [SerializeField] GameObject fxSlot;

        protected override void Awake()
        {
            base.Awake();
            SetFx(false);
        }

        protected override void OnLocalize()
        {
        }

        public void Set(string title, string optionValue, string value)
        {
            labelOptionName.Text = title;
            labelOptionValue.Text = optionValue;
            labelValue.SetActive(!string.IsNullOrEmpty(value));
            labelValue.Text = value;
        }       

        public void SetFx(bool isActive)
        {
            if (fxSlot)
                fxSlot.SetActive(isActive);
        }

        public void ShowFx()
        {
            SetFx(false);
            SetFx(true);
        }
    }
}