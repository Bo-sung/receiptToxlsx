using UnityEngine;

namespace Ragnarok
{
    public sealed class BattleOptionSlot : MonoBehaviour
    {
        [SerializeField] UILabelHelper labOptionName;
        [SerializeField] UILabelHelper labOptionValue;

        public void Init(string optionName, int value)
        {
            labOptionName.Text = optionName;
            labOptionValue.Text = $"+{value}";
        }

        public void Init(BattleOption option)
        {
            labOptionName.Text = option.battleOptionType.ToText();
            if (option.value1 > 0)
            {
                labOptionValue.Text = $"+{option.value1}";
            }
            else
            {
                labOptionValue.Text = $"+{option.value2}%";
            }
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}