using UnityEngine;

namespace Ragnarok
{
    public class UIButtonWithToggleAdvanced : UIButtonWithToggle
    {
        [SerializeField] UILabelHelper labelOff;

        public int OffLocalKey
        {
            set { OffText = value.ToText(); }
        }

        public string OffText
        {
            get
            {
                if (labelOff)
                    return labelOff.Text;

                return string.Empty;
            }
            set
            {
                if (labelOff)
                    labelOff.Text = value;
            }
        }
    }
}