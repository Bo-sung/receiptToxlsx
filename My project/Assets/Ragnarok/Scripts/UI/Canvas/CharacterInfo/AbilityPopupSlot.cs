using UnityEngine;
using System.Collections;

namespace Ragnarok
{
    public class AbilityPopupSlot : MonoBehaviour
    {
        public class Input
        {
            public string abilityName;
            public string abilityValue;

            public Input(string abilityName, string abilityValue)
            {
                this.abilityName = abilityName;
                this.abilityValue = abilityValue;
            }
        }

        [SerializeField] UILabel nameLabel;
        [SerializeField] UILabel nameValue;
        [SerializeField] GameObject bg;

        public void SetData(Input input, int index)
        {
            nameLabel.text = input.abilityName;
            nameValue.text = input.abilityValue;
            bg.SetActive((index % 2) == 1);
        }
    }
}